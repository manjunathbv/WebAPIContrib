﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApiContrib.MessageHandlers
{
    public class NotAcceptableMessageHandler : DelegatingHandler
    {
        private const string allMediaTypesRange = "*/*";

    	private readonly HttpConfiguration configuration;

		public NotAcceptableMessageHandler(HttpConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			this.configuration = configuration;
		}

		public NotAcceptableMessageHandler(HttpConfiguration configuration, HttpMessageHandler innerHandler)
			: base(innerHandler)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			this.configuration = configuration;
		}

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var acceptHeader = request.Headers.Accept;

            if (!IsRequestedMediaTypeAccepted(acceptHeader))
                return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.NotAcceptable));

            return base.SendAsync(request, cancellationToken);
        }

        private bool IsRequestedMediaTypeAccepted(HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> acceptHeader)
        {
            return configuration
                .Formatters
                .Any(formatter => acceptHeader.Any(mediaType => FormatterSuportsMediaType(mediaType, formatter)));
        }

        private static bool FormatterSuportsMediaType(MediaTypeWithQualityHeaderValue mediaType, MediaTypeFormatter formatter)
        {
            var supportsMediaType = formatter.SupportedMediaTypes.Contains(mediaType);
            var supportsTypeGroup = formatter.SupportedMediaTypes.Any(mt =>
                                                                          {
                                                                              var splitMediaType = mt.MediaType.Split('/');
                                                                              var type = splitMediaType.First();
                                                                              return mediaType.MediaType.StartsWith(type);
                                                                          });

            var isTypeGroup = mediaType.MediaType.Split('/').Last() == "*";
            var isAllMediaType = mediaType.MediaType == allMediaTypesRange;


            return isAllMediaType || supportsMediaType || (isTypeGroup && supportsTypeGroup);
        }
    }
}
