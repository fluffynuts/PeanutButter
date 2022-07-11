using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using PeanutButter.TestUtils.AspNetCore.Builders;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore
{
    /// <summary>
    /// Starts off a builder for your ApiController
    /// (controller inheriting from ControllerBase only)
    /// </summary>
    public static class ControllerBuilder
    {
        /// <summary>
        /// Create the builder for your controller
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public static ControllerBuilder<TController> For<TController>()
            where TController : ControllerBase
        {
            return new ControllerBuilder<TController>();
        }
    }

    /// <summary>
    /// Builds your
    /// </summary>
    /// <typeparam name="TController"></typeparam>
    public class ControllerBuilder<TController>
        : Builder<ControllerBuilder<TController>, TController>
        where TController : ControllerBase
    {
        /// <summary>
        /// Builds the default instance
        /// </summary>
        /// <returns></returns>
        public new TController BuildDefault()
        {
            return Build();
        }

        /// <inheritdoc />
        public ControllerBuilder()
        {
            WithControllerContext(new ControllerContext())
                .WithRouteData(new RouteData())
                .WithActionDescriptor(new ControllerActionDescriptor())
                .WithHttpContext(HttpContextBuilder.Create()
                    .WithRequestServices(new MinimalServiceProvider())
                    .Build()
                )
                .WithModelMetadataProvider(
                    () => new DefaultModelMetadataProvider(
                        new DefaultCompositeMetadataDetailsProvider(
                            new IMetadataDetailsProvider[0]
                        )
                    )
                )
                .WithModelValidator(() => new DefaultObjectModelValidator())
                .WithTempDataDictionaryFactory(() => new TempDataDictionaryFactory(
                        new SessionStateTempDataProvider()
                    )
                )
                .WithOptions(() => new DefaultOptions());
        }

        /// <summary>
        /// Sets the controller context for your controller
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithControllerContext(
            ControllerContext ctx
        )
        {
            return With(
                o => o.ControllerContext = ctx
            );
        }

        /// <summary>
        /// Sets the action descriptor for your controller
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithActionDescriptor(
            ControllerActionDescriptor actionDescriptor
        )
        {
            return With(
                o => o.ControllerContext.ActionDescriptor = actionDescriptor
            );
        }

        /// <summary>
        /// Sets the route data for your controller
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithRouteData(RouteData data)
        {
            return With(
                o => o.ControllerContext.RouteData = data
            );
        }

        /// <summary>
        /// Sets the http context for your controller
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithHttpContext(
            HttpContext context
        )
        {
            return With(o => o.ControllerContext.HttpContext = context)
                .WithActionContext(ctx => new ActionContext(
                        ctx.HttpContext,
                        ctx.RouteData,
                        ctx.ActionDescriptor
                    )
                );
        }

        /// <summary>
        /// Set the options for the controller
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithOptions(
            IOptions<MvcOptions> options
        )
        {
            return WithOptions(() => options);
        }

        /// <summary>
        /// Set the options for the controller (late-resolved singleton)
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithOptions(
            Func<IOptions<MvcOptions>> factory
        )
        {
            return WithRegistration(
                (sp, _) => sp.RegisterSingleton(factory)
            );
        }

        /// <summary>
        /// Set the temp data dictionary factory for the controller
        /// </summary>
        /// <param name="tempDataDictionaryFactory"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithTempDataDictionaryFactory(
            ITempDataDictionaryFactory tempDataDictionaryFactory
        )
        {
            return WithTempDataDictionaryFactory(() => tempDataDictionaryFactory);
        }

        /// <summary>
        /// Set the temp data dictionary factory for the controller (late-resolved singleton)
        /// </summary>
        /// <param name="factoryFactory"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithTempDataDictionaryFactory(
            Func<ITempDataDictionaryFactory> factoryFactory
        )
        {
            return WithRegistration(
                (sp, _) => sp.RegisterSingleton(factoryFactory)
            );
        }

        /// <summary>
        /// Set the model validator
        /// </summary>
        /// <param name="validator"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithModelValidator(
            IObjectModelValidator validator
        )
        {
            return WithModelValidator(() => validator);
        }

        /// <summary>
        /// Set the model validator (late-resolved singleton)
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithModelValidator(
            Func<IObjectModelValidator> factory
        )
        {
            return WithRegistration(
                (sp, _) => sp.RegisterSingleton(factory)
            );
        }

        /// <summary>
        /// Set the model metadata provider
        /// </summary>
        /// <param name="modelMetadataProvider"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithModelMetadataProvider(
            Func<IModelMetadataProvider> modelMetadataProvider
        )
        {
            return WithRegistration(
                (sp, _) => sp.RegisterSingleton(modelMetadataProvider)
            );
        }

        /// <summary>
        /// Set the model metadata provider (late-bound singleton)
        /// </summary>
        /// <param name="modelMetadataProvider"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithModelMetadataProvider(
            IModelMetadataProvider modelMetadataProvider
        )
        {
            return WithModelMetadataProvider(() => modelMetadataProvider);
        }

        /// <summary>
        /// Set the action context (will be automatically called when you set
        /// the http context). This variant will have access to the controller
        /// so it can access any property of said controller.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithActionContext(
            Func<ControllerContext, ActionContext> factory
        )
        {
            return WithRegistration(
                (sp, o) => sp.RegisterSingleton(() => factory(o.ControllerContext))
            );
        }

        /// <summary>
        /// Set the action context (late-bound singleton)
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public ControllerBuilder<TController> WithActionContext(
            ActionContext actionContext
        )
        {
            return WithRegistration(
                (sp, _) => sp.RegisterSingleton(() => actionContext)
            );
        }

        private ControllerBuilder<TController> WithRegistration(
            Action<IMinimalServiceProvider, TController> registration
        )
        {
            return With(
                o =>
                {
                    var context = o.ControllerContext?.HttpContext
                        ?? throw new InvalidOperationException("HttpContext not set up on ControllerContext yet");
                    if (context.RequestServices is not IMinimalServiceProvider minimalServiceProvider)
                    {
                        throw new InvalidOperationException(
                            $"Cannot late-bind services the RequestServices on HttpContext is not a {nameof(IMinimalServiceProvider)}"
                        );
                    }

                    registration(minimalServiceProvider, o);
                }
            );
        }

        /// <inheritdoc />
        [Obsolete("Does nothing")]
        public override ControllerBuilder<TController> Randomize()
        {
            return this;
        }
    }
}