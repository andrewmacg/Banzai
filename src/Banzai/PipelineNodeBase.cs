﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Banzai
{
    /// <summary>
    /// This node runs children serially as defined by their current order (as added).
    /// This node is used for constructing pipelines. The node will not complete until all children complete or an error is encountered.
    /// </summary>
    public interface IPipelineNodeBase<T> : IMultiNode<T>
    {

    }

    /// <summary>
    /// Class from which to derive custom pipeline nodes.
    /// This node runs children serially as defined by their current order (as added).
    /// This node is used for constructing pipelines. The node will not complete until all children complete or an error is encountered.
    /// </summary>
    public abstract class PipelineNodeBase<T> : MultiNode<T>, IPipelineNodeBase<T>
    {
        /// <summary>
        /// Executes child nodes of the current node.
        /// </summary>
        /// <param name="context">Current ExecutionContext.</param>
        /// <returns>NodeResultStatus representing the current node result.</returns>
        protected override async Task<NodeResultStatus> ExecuteChildrenAsync(ExecutionContext<T> context)
        {
            var results = new List<NodeResult<T>>();

            LogWriter.Debug("Running each child node in the pipeline sequentially.");
            foreach (var childNode in Children)
            {
                LogWriter.Debug("Running child node.");
                var result = await childNode.ExecuteAsync(context).ConfigureAwait(false);

                results.Add(result);

                if (result.Status == NodeResultStatus.Failed && !context.EffectiveOptions.ContinueOnFailure)
                {
                    break;
                }
            }

            return results.AggregateNodeResults(context.EffectiveOptions);
        }
    }
}