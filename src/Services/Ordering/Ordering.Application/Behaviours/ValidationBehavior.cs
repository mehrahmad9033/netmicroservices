using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = Ordering.Application.Exceptions.ValidationException;
namespace Ordering.Application.Behaviours
{
	public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	{
		private readonly IEnumerable<IValidator<TRequest>> _vaidators;

		public ValidationBehavior(IEnumerable<IValidator<TRequest>> vaidators)
		{
			_vaidators = vaidators ?? throw new ArgumentNullException(nameof(vaidators));
		}

		public async Task<TResponse> Handle(TRequest request, 
			CancellationToken cancellationToken, 
			RequestHandlerDelegate<TResponse> next)
		{
			if (_vaidators.Any())
			{
				var context = new ValidationContext<TRequest>(request);
				var validationResults = await Task.WhenAll(_vaidators.Select(v => v.ValidateAsync(context, cancellationToken)));
				var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

				if (failures.Count > 0)
				{
					throw new ValidationException(failures);
				}
			}
			return await next();
		}
	}
}
