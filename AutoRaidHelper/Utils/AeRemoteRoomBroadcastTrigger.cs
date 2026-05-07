using System.Text;
using AEAssist.Helper;
using AutoRaidHelper.Settings;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;
using ECommons.DalamudServices;

namespace AutoRaidHelper.Utils;

/// <summary>
/// 监听小队/团队频道中的严格匹配口令，并自动广播当前 AE 遥控房间号。
/// </summary>
public sealed class AeRemoteRoomBroadcastTrigger : IDisposable
{
    public AeRemoteRoomBroadcastTrigger()
    {
        Svc.Chat.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        Svc.Chat.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(IHandleableChatMessage message)
    {
        try
        {
            var settings = FullAutoSettings.Instance.AeRemoteRoomBroadcastSettings;
            if (!settings.Enabled)
                return;

            if (message.LogKind != (XivChatType)14 && message.LogKind != (XivChatType)15)
                return;

            var roomId = RemoteControlHelper.RoomId;
            if (string.IsNullOrEmpty(roomId))
                return;

            var triggerText = settings.TriggerText ?? string.Empty;
            if (string.IsNullOrEmpty(triggerText))
                return;

            if (!string.Equals(message.Message.TextValue, triggerText, StringComparison.Ordinal))
                return;

            var channelPrefix = message.LogKind == (XivChatType)15 ? "/a" : "/p";
            var encodedRoomId = Convert.ToBase64String(Encoding.UTF8.GetBytes(roomId));
            ChatHelper.SendMessage($"{channelPrefix} {encodedRoomId}");
        }
        catch (Exception ex)
        {
            LogHelper.Error($"[ARH] AE 房间号自动广播失败: {ex.Message}");
        }
    }
}
