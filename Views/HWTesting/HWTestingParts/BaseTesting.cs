namespace fyp_MDPHelperApp.Views.HWTesting.HWTestingParts;

public abstract class BaseTesting
{
    public abstract void ShowMenu(Action<string> logAction);

    public virtual bool IsCommandValid(string command)
    {
        return true;
    }

    public virtual void ProcessData(string data)
    {
    }

    public virtual void ClearData()
    {
    }

    public virtual string ProcessCommand(string command)
    {
        return command;
    }
}