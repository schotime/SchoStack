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
        HtmlTag Build<T>(T value);
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

            return pipeline.BuildHtmlTag();
        }

        public HtmlTag Build<T>(T value)
        {
            var pipeline = new ConventionPipeline(new ValueRequestData<T>(_requestData, value), _builders);
            return pipeline.BuildHtmlTagNoPipe();
        }

        internal HtmlTag BuildHtmlTagNoPipe()
        {
            foreach (var builder in _builders.Where(x => x.Condition(_requestData)))
            {
                if (builder.BuilderFunc != null)
                {
                    var tag = builder.BuilderFunc(_requestData);
                    if (tag != null)
                        return tag;
                }
            }

            return null;
        }

        internal HtmlTag BuildHtmlTag()
        {
            foreach (var builder in _builders.Where(x => x.Condition(_requestData)))
            {
                var pipeLineFunc = builder.BuilderFuncPipeline;
                if (pipeLineFunc != null)
                {
                    var tag = pipeLineFunc(_requestData, this);
                    if (tag != null)
                        return tag;
                }
                else
                {
                    var tag = builder.BuilderFunc(_requestData);
                    if (tag != null)
                        return tag;
                }
            }

            return null;
        }
    }
}