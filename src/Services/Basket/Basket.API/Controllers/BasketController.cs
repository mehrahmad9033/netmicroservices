using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Message.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class BasketController : ControllerBase
	{
		private readonly IBasketRepository _repository;
		private readonly DiscountGrpcService _discountgrpcrepository;
		private readonly IMapper _mapper;
		private readonly IPublishEndpoint _publishEndpoint;

		public BasketController(IBasketRepository repository, DiscountGrpcService discountgrpcrepository, IMapper mapper, IPublishEndpoint publishEndpoint)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_discountgrpcrepository = discountgrpcrepository ?? throw new ArgumentNullException(nameof(discountgrpcrepository));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		[HttpGet("{userName}",Name ="Getbasket")]
		[ProducesResponseType(typeof(ShoppingCart),(int)HttpStatusCode.OK)]
		public async Task<ActionResult<ShoppingCart>> GetBasket(string username)
		{
			var basket = await _repository.GetBasket(username);
			return Ok(basket??new ShoppingCart(username));
		}
		[HttpPost]
		[ProducesResponseType(typeof(ShoppingCart),(int)HttpStatusCode.OK)]
		public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
		{
			//=========TODO: Communicate with Discont.gRPC 
			//=========      and Calculate latest prices of products
			//========= Consume Discount GRPC
			foreach (var item in basket.Items)
			{
				var coupon = await _discountgrpcrepository.GetDiscount(item.ProductName);
				item.Price -= coupon.Amount;
			}
			return Ok(await _repository.UpdateBasket(basket));
		}
		[HttpDelete("{username}",Name ="DeleteBasket")]
		[ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> DeleteBasket(string userName)
		{
			await _repository.DeleteBasket(userName);
			return Ok();
		}
		[Route("[action]")]
		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketcheckout)
		{
			// get existing basket with totalprice
			// create absketcheckoutevent -- set total price on basketcheckout eventmessage
			// send checkout event to rabbitmq
			// remove from basket

			var basket = await _repository.GetBasket(basketcheckout.UserName);
			if (basket == null)
			{
				return BadRequest();
			}

			//===send checkout event to rabbitmq
			var eventmessage = _mapper.Map<BasketCheckoutEvent>(basketcheckout);
			eventmessage.TotalPrice = basket.TotalPrice;
			await _publishEndpoint.Publish(eventmessage);

			//====removing basket from redi
			await _repository.DeleteBasket(basket.UserName);


			return Accepted();
		}
	}
}
