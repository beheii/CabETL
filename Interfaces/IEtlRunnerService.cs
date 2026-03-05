namespace CabETL.Interfaces
{
    public interface IEtlRunnerService
    {
        int Run(string csvPath, string duplicatesCsvPath);
    }
}