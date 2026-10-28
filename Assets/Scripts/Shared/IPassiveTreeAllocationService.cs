public interface IPassiveTreeAllocationService
{
    void InitializeServer();
    void ShutdownServer();
    bool TryAllocateNode(string nodeName);
    bool TryRefundNode(string nodeName);
}