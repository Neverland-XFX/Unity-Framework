using System.Collections.Generic;
using System.Linq.Expressions;

namespace GameLogic.Binding.Paths
{
    public interface IExpressionPathFinder
    {
        List<Path> FindPaths(LambdaExpression expression);
        
    }
}