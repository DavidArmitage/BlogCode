using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.PublishedModels;
using Website.Core.Services;

namespace Website.Core.Events
{
    public class PublishedEvent : INotificationHandler<ContentPublishedNotification>
    {
        private readonly ISiteService _siteService;
        private readonly CacheTagHelperMemoryCacheFactory _cacheFactory;
        private readonly ILogger<PublishedEvent> _logger;

        public PublishedEvent(ISiteService siteService, CacheTagHelperMemoryCacheFactory cacheFactory, ILogger<PublishedEvent> logger) : base()
        {
            _siteService = siteService;
            _cacheFactory = cacheFactory;
            _logger = logger;
        }

        public void Handle(ContentPublishedNotification notification)
        {
            foreach (IContent node in notification.PublishedEntities)
            {
                ClearSharedPartialViewCache(node);

                if (node.ContentType.Alias.Equals("contentPage"))
                {
                    
                }
            }
        }

        public void ClearSharedPartialViewCache(IContent node)
        {
            IPublishedContent content = _siteService.GetContentById(node.Id);
            if (content != null && content is ICacheSettingsComposition cacheSettings)
            {
                var cache = _cacheFactory.Cache;
                cache.RemoveCacheTagKeyBy_varyBy(cacheSettings.ClearCachedSharedPartialViewsOnPublish);
            }
        }
    }
}
