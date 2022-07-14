using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Builds ControllerActionDescriptor instances
    /// </summary>
    public class ControllerActionDescriptorBuilder
        : Builder<ControllerActionDescriptorBuilder, ControllerActionDescriptor>
    {
        /// <summary>
        /// Sets the controller name on the action descriptor
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ControllerActionDescriptorBuilder WithControllerName(string name)
        {
            return With(
                o => o.ControllerName = name
            );
        }

        /// <summary>
        /// Sets the action name on the action descriptor
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ControllerActionDescriptorBuilder WithActionName(string name)
        {
            return With(
                o => o.ActionName = name
            );
        }

        /// <summary>
        /// Sets the method info on the action descriptor
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public ControllerActionDescriptorBuilder WithMethodInfo(MethodInfo methodInfo)
        {
            return With(
                o => o.MethodInfo = methodInfo
            );
        }

        /// <summary>
        /// Sets the controller type on the action descriptor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ControllerActionDescriptorBuilder WithControllerType(Type type)
        {
            return With(
                o => o.ControllerTypeInfo = type.GetTypeInfo()
            );
        }

        /// <summary>
        /// Sets the controller type on the action descriptor
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public ControllerActionDescriptorBuilder WithControllerType(TypeInfo typeInfo)
        {
            return With(
                o => o.ControllerTypeInfo = typeInfo
            );
        }
    }
}