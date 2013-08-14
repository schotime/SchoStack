using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FubuCore.Reflection;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public interface IConventionPipeline
    {
        HtmlTag Build<T>(Expression<Func<T, object>> prop);
    }

    public class ConventionPipeline : IConventionPipeline
    {
        private readonly RequestData _requestData;
        private readonly IList<Builder> _builders;

        public ConventionPipeline(RequestData requestData, IList<Builder> builders)
        {
            _requestData = requestData;
            _builders = builders;
        }

        public HtmlTag Build<T>(Expression<Func<T, object>> prop)
        {
            var req = TagGenerator.BuildRequestData(_requestData.ViewContext, prop);
            req.Accessor = _requestData.Accessor.GetChildAccessor(prop);
            
            var pipeline = new ConventionPipeline(req, _builders);

            return BuildHtmlTag(pipeline);
        }

        public HtmlTag BuildHtmlTag(ConventionPipeline pipeline)
        {
            foreach (var builder in _builders.Where(x => x.Condition(pipeline._requestData)))
            {
                var pipeLineFunc = builder.BuilderFuncPipeline;
                if (pipeLineFunc != null)
                {
                    var tag = pipeLineFunc(pipeline._requestData, pipeline);
                    if (tag != null)
                        return tag;
                }
                else
                {
                    var tag = builder.BuilderFunc(pipeline._requestData);
                    if (tag != null)
                        return tag;
                }
            }

            return null;
        }
    }
}