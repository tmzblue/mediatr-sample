using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatrSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var sp = new ServiceCollection()
            .AddMediatR(typeof(Program))
            .AddScoped<IPipelineBehavior<SaveCommand, string>, SomeBehavior>()
            .AddSingleton<SomeController>()
            .BuildServiceProvider();

            var someController = sp.GetService<SomeController>();
            var someControllerResult = someController.Save(new SaveCommand
            {
                Thing = "something..."
            });

            Console.WriteLine(someControllerResult);
        }
    }

    class SomeController
    {
        readonly ISender sender;

        public SomeController(ISender sender)
        {
            this.sender = sender;
        }

        public string Save(SaveCommand thing)
        {
            return sender.Send(thing).Result;
        }
    }

    class SaveCommand : IRequest<string>
    {
        public string Thing;
    }

    class SaveCommandHandler : IRequestHandler<SaveCommand, string>
    {
        public Task<string> Handle(SaveCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Saving {request.Thing}");
            return Task.FromResult("Saved");
        }
    }

    class SomeBehavior : IPipelineBehavior<SaveCommand, string>
    {
        public async Task<string> Handle(SaveCommand request, CancellationToken cancellationToken, RequestHandlerDelegate<string> next)
        {
            request.Thing += " (injected by behavior)";
            var result = await next();
            result += " (result manipulation)";
            return result;
        }
    }
}
