namespace PallasBot.Domain.Constants;

public static class MaaConstants
{
    public const string Organization = "MaaAssistantArknights";

    public const string MainRepository = "MaaAssistantArknights";
    public const string BackendCenterRepository = "MaaBackendCenter";
    public const string CopilotFrontendRepository = "maa-copilot-frontend";
    public const string MaaCliRepository = "maa-cli";
    public const string MaaMacGuiRepository = "MaaMacGui";

    public static IEnumerable<string> Repositories =>
    [
        MainRepository,
        BackendCenterRepository,
        CopilotFrontendRepository,
        MaaCliRepository,
        MaaMacGuiRepository
    ];
}
