using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Features.Orders.Commands.CheckoutOrder
{
	public class CheckoutOrderCommandValidator:AbstractValidator<CheckoutOrderCommand>
	{
		public CheckoutOrderCommandValidator()
		{
			RuleFor(p => p.UserName)
				.NotEmpty().WithMessage("{User Name} is Required")
				.NotNull()
				.MaximumLength(50).WithMessage("{UserName} must not exceed 50 characters");

			RuleFor(p => p.EmailAddress)
							.NotEmpty().WithMessage("{EmailAddress} is Required");

			RuleFor(p => p.TotalPrice)
				.NotEmpty().WithMessage("{TotalPrice} is Required")
				.GreaterThan(0).WithMessage("{TotalPrice} must be greater than zero");

		}
	}
}
