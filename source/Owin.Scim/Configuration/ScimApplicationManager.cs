﻿namespace Owin.Scim.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    using DryIoc;
    using DryIoc.WebApi;

    using Endpoints;

    using Middleware;

    using Model;

    using NContext.Configuration;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using Serialization;

    public class ScimApplicationManager : IApplicationComponent
    {
        private readonly IAppBuilder _AppBuilder;

        private readonly IContainer _IocContainer;

        private readonly Action<ScimServerConfiguration> _ConfigureScimServerAction;

        private bool _IsConfigured;

        public ScimApplicationManager(
            IAppBuilder appBuilder,
            IContainer iocContainer,
            Action<ScimServerConfiguration> configureScimServerAction)
        {
            _AppBuilder = appBuilder;
            _IocContainer = iocContainer;
            _ConfigureScimServerAction = configureScimServerAction;
        }

        public bool IsConfigured
        {
            get { return _IsConfigured; }
            private set { _IsConfigured = value; }
        }

        public void Configure(ApplicationConfigurationBase applicationConfiguration)
        {
            if (IsConfigured) return;

            var serverConfiguration = new ScimServerConfiguration();
            applicationConfiguration.CompositionContainer.ComposeExportedValue(serverConfiguration);
            _IocContainer.RegisterInstance(serverConfiguration, Reuse.Singleton);

            var typeDefinitions = applicationConfiguration.CompositionContainer.GetExportedValues<IScimTypeDefinition>();
            foreach (var typeDefinition in typeDefinitions)
                serverConfiguration.AddTypeDefiniton(typeDefinition);

            _ConfigureScimServerAction?.Invoke(serverConfiguration);

            // Set default public origin
            if (serverConfiguration.PublicOrigin == null && _AppBuilder.Properties.ContainsKey("host.Addresses"))
            {
                var items = ((IList<IDictionary<string, object>>)_AppBuilder.Properties["host.Addresses"])[0];
                var port = items.ContainsKey("port")
                    ? int.Parse(items["port"].ToString())
                    : -1;

                var uriBuilder = new UriBuilder(
                    items.ContainsKey("scheme") ? items["scheme"].ToString() : null,
                    items.ContainsKey("host") ? items["host"].ToString() : null,
                    (port != 80 && port != 443) ? port : -1,
                    items.ContainsKey("path") ? items["path"].ToString() : null);

                serverConfiguration.PublicOrigin = uriBuilder.Uri;
            }

            if (serverConfiguration.RequireSsl)
                _AppBuilder.Use<RequireSslMiddleware>();

            var httpConfig = CreateHttpConfiguration();
            _IocContainer.WithWebApi(httpConfig);

            _AppBuilder.UseWebApi(httpConfig);

            IsConfigured = true;
        }

        private static HttpConfiguration CreateHttpConfiguration()
        {
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            httpConfiguration.MapHttpAttributeRoutes();

            var settings = httpConfiguration.Formatters.JsonFormatter.SerializerSettings;
            settings.Converters.Add(new StringEnumConverter());
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            settings.ContractResolver = new ScimContractResolver
            {
                IgnoreSerializableAttribute = true,
                IgnoreSerializableInterface = true
            };

            httpConfiguration.ParameterBindingRules.Insert(
                0,
                descriptor =>
                {
                    if (typeof(Resource).IsAssignableFrom(descriptor.ParameterType))
                        return new ResourceParameterBinding(
                            descriptor,
                            descriptor.Configuration.DependencyResolver.GetService(typeof(ISchemaTypeFactory)) as ISchemaTypeFactory);

                    return null;
                });

            // refer to https://tools.ietf.org/html/rfc7644#section-3.1
            httpConfiguration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("application/scim+json"));

            httpConfiguration.Services.Replace(
                typeof(IHttpControllerTypeResolver),
                new DefaultHttpControllerTypeResolver(IsControllerType));

            httpConfiguration.Filters.Add(
                new ModelBindingResponseAttribute());

            return httpConfiguration;
        }

        private static bool IsControllerType(Type t)
        {
            return
                typeof(ScimControllerBase).IsAssignableFrom(t) &&
                t != null &&
                t.IsClass &&
                t.IsVisible &&
                !t.IsAbstract &&
                typeof(IHttpController).IsAssignableFrom(t) &&
                HasValidControllerName(t);
        }

        private static bool HasValidControllerName(Type controllerType)
        {
            string controllerSuffix = DefaultHttpControllerSelector.ControllerSuffix;
            return controllerType.Name.Length > controllerSuffix.Length &&
                controllerType.Name.EndsWith(controllerSuffix, StringComparison.OrdinalIgnoreCase);
        }
    }
}