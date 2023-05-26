using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class RequestDelegateTestArenaBuilder
    {
        /// <summary>
        /// Create the arena for fluent syntax usage
        /// </summary>
        /// <returns></returns>
        public static RequestDelegateTestArenaBuilder Create()
        {
            return new RequestDelegateTestArenaBuilder();
        }

        /// <summary>
        /// Builds the default RequestDelegateTestArena
        /// </summary>
        /// <returns></returns>
        public static RequestDelegateTestArena BuildDefault()
        {
            return Create().Build();
        }

        private Func<HttpContext, Task> _logic = NoOp;
        private readonly List<Action<HttpContextBuilder>> _contextMutators = new();
        private HttpContext _httpContext;

        private static Task NoOp(HttpContext ctx)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Build the arena
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public RequestDelegateTestArena Build()
        {
            return _httpContext is null
                ? new RequestDelegateTestArena(
                    _logic,
                    RunMutators
                )
                : new RequestDelegateTestArena(_logic, _httpContext);
        }


        private void RunMutators(HttpContextBuilder builder)
        {
            foreach (var mutator in _contextMutators)
            {
                mutator(builder);
            }
        }

        /// <summary>
        /// Generate the request delegate arena for a random OPTIONS request
        /// </summary>
        /// <returns></returns>
        public RequestDelegateTestArenaBuilder ForOptionsRequest()
        {
            return WithContextMutator(
                builder => builder.With(
                    o => o.WithRequestMutator(
                        r => r.Method = "OPTIONS"
                    )
                )
            );
        }

        /// <summary>
        /// Fluent mechanism for adding an http context mutation
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public RequestDelegateTestArenaBuilder WithContextMutator(
            Action<HttpContextBuilder> mutator
        )
        {
            if (mutator is not null)
            {
                _contextMutators.Add(mutator);
            }

            return this;
        }

        /// <summary>
        /// Add a mutation on the request for the context
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public RequestDelegateTestArenaBuilder WithRequestMutator(
            Action<HttpRequest> mutator
        )
        {
            if (mutator is not null)
            {
                _contextMutators.Add(
                    builder =>
                        builder.WithRequestMutator(mutator)
                );
            }

            return this;
        }

        /// <summary>
        /// Set the entire request for the context
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RequestDelegateTestArenaBuilder WithRequest(
            HttpRequest request
        )
        {
            _contextMutators.Add(
                builder => builder.WithRequest(request)
            );
            return this;
        }

        /// <summary>
        /// Fluent mechanism for setting the delegate logic (overrides any existing logic)
        /// </summary>
        /// <param name="logic"></param>
        /// <returns></returns>
        public RequestDelegateTestArenaBuilder WithDelegateLogic(
            Action<HttpContext> logic
        )
        {
            return WithDelegateLogic(RequestDelegateTestArena.WrapSynchronousLogic(logic));
        }

        private RequestDelegateTestArenaBuilder WithDelegateLogic(
            Func<HttpContext, Task> logic
        )
        {
            _logic = logic;
            return this;
        }

        /// <summary>
        /// Fluent mechanism for setting the HttpContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public RequestDelegateTestArenaBuilder WithContext(HttpContext context)
        {
            _httpContext = context;
            return this;
        }
    }
}