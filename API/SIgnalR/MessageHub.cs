using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SIgnalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMapper mapper;
        private readonly IMessageRepository messageRepository;
        private readonly IUserRepository userRepository;
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker tracker;
        public MessageHub(IMessageRepository messageRepository,
            IMapper mapper, IUserRepository userRepository,
            IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            this.tracker = tracker;
            this.presenceHub = presenceHub;
            this.userRepository = userRepository;
            this.messageRepository = messageRepository;
            this.mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(httpContext.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Groups(groupName).SendAsync("UpdatedGroup", group);

            var messageThread = messageRepository.GetMessageThread(httpContext.User.GetUsername(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messageThread);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var senderUsername = Context.User.GetUsername();

            if (createMessageDto.RecipientUsername == senderUsername)
                throw new HubException("Cannot send message to yourself.");

            var sender = await userRepository.GetUserByUsernameAsync(senderUsername);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Cannot find recepient");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                Content = createMessageDto.Content,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await messageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(c => c.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connection = await tracker.GetConnectionsForUser(recipient.UserName);
                if(connection != null)
                {
                    await presenceHub.Clients.Clients(connection).SendAsync("NewMessageReceived", new {
                        username = senderUsername,
                        knownAs = sender.KnownAs
                    });
                }
            }

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await messageRepository.SaveAllAsync())
                return group;

            throw new HubException("Failed to join hub");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            messageRepository.RemoveConnection(connection);
            if(await messageRepository.SaveAllAsync()) return group;

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            var strCompare = string.CompareOrdinal(caller, other) < 0;
            return strCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}