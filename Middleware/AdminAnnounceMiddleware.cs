using VampireCommandFramework;
using System.Reflection;
using RevealForge.Utils; 
using System;
using Unity.Entities;
using Unity.Collections;
using ProjectM;

namespace RevealForge.Middlewares
{
    public class AdminAnnounceMiddleware : CommandMiddleware
    {
        public AdminAnnounceMiddleware() { }

        public override void AfterExecute(ICommandContext ctx, CommandAttribute commandAttr, MethodInfo method)
        {
            if (ctx == null || commandAttr == null) return;
            if (!ctx.IsAdmin || !commandAttr.AdminOnly) return;

            string adminName = ctx.Name;
            string commandExecutedString;

            if (ctx is ChatCommandContext chatCtx && chatCtx.Event != null && !string.IsNullOrEmpty(chatCtx.Event.Message))
            {
                commandExecutedString = chatCtx.Event.Message;
            }
            else
            {
                commandExecutedString = $".{commandAttr.Name}";
                if (method != null && method.GetParameters().Length > (ctx is ChatCommandContext ? 1 : 0))
                {
                    commandExecutedString += " [with arguments]";
                }
            }

            if (commandExecutedString.Length > 100) commandExecutedString = commandExecutedString.Substring(0, 97) + "...";
            commandExecutedString = commandExecutedString.Replace("\n", " ").Replace("\r", " ");

            string announcement = $"{ChatColors.FormatText("Admin")} {ChatColors.FormatAdminName(adminName)} {ChatColors.FormatText("executed")} {ChatColors.FormatCommand(commandExecutedString)}";
            BroadcastMessageToAllPlayers(announcement);
        }

        private void BroadcastMessageToAllPlayers(string message)
        {
            EntityManager entityManager;
            try
            {
                if (VWorld.Server == null || !VWorld.Server.IsCreated) return;
                entityManager = VWorld.Server.EntityManager;
            }
            catch (Exception) { return; }

            if (entityManager.World == null || !entityManager.World.IsCreated) return;

            try
            {
                FixedString512Bytes fixedMessage = message;
                ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref fixedMessage);
            }
            catch (System.Exception) {  }
        }
    }
}