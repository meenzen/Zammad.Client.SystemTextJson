using TUnit.Core.Interfaces;
using Zammad.Client.IntegrationTests.Infrastructure;

[assembly: ParallelLimiter<ProcessorCountLimit>]

namespace Zammad.Client.IntegrationTests.Infrastructure;

public class ProcessorCountLimit : IParallelLimit
{
    public int Limit => Environment.ProcessorCount;
}
