using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using Restaurant.Extensions;
using Restaurant.Models;
using Restaurant.Repository;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Policy;

namespace Restaurant.Hubs
{
    /// <summary>
    /// SinalR README
    /// 
    /// Clients.Caller: Gửi tin nhắn đến client đã gọi phương thức trong Hub.
    /// await Clients.Caller.SendAsync("MethodName", parameters);
    /// 
    /// Clients.All: Gửi tin nhắn đến tất cả các client đang kết nối.
    /// await Clients.All.SendAsync("MethodName", parameters);
    /// 
    /// Clients.AllExcept: Gửi tin nhắn đến tất cả các client trừ những client có ID được chỉ định.
    /// var excludedConnectionIds = new[] { "connectionId1", "connectionId2" };
    /// await Clients.AllExcept(excludedConnectionIds).SendAsync("MethodName", parameters);
    /// 
    /// Clients.Group: Gửi tin nhắn đến tất cả các client trong một nhóm cụ thể.
    /// await Clients.Group("groupName").SendAsync("MethodName", parameters);
    /// 
    /// Clients.User: Gửi tin nhắn đến client cụ thể dựa trên tên người dùng (thông qua UserIdentifier).
    /// await Clients.User("userName").SendAsync("MethodName", parameters);
    /// 
    /// Clients.User: Gửi tin nhắn đến một danh sách các người dùng.
    /// var userNames = new[] { "user1", "user2" };
    /// await Clients.Users(userNames).SendAsync("MethodName", parameters);
    /// 
    /// Clients.Others: Gửi tin nhắn đến tất cả client khác ngoại trừ client đã gọi phương thức
    /// await Clients.Others.SendAsync("MethodName", parameters);
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        /// <summary>
        /// Danh sách kết nối của người dùng
        /// Key: userId
        /// Value: connectionId
        /// </summary>
        private static readonly ConcurrentDictionary<long, string> UserConnections = new ConcurrentDictionary<long, string>();

        /// <summary>
        /// Danh sách các cuộc trò chuyện của người dùng
        /// Key: conversationId
        /// </summary>
        private static readonly ConcurrentDictionary<int, HashSet<string>> UserConversations = new ConcurrentDictionary<int, HashSet<string>>();

        private readonly DataContext _context;

        public ChatHub(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            if (int.TryParse(Context.UserIdentifier, out int userId))
            {
                UserConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (int.TryParse(Context.UserIdentifier, out int userId))
            {
                UserConnections.TryRemove(userId, out _);
            }
            var connectionId = Context.ConnectionId;
            var conversation = UserConversations.FirstOrDefault(x => x.Value.Contains(connectionId));
            if (conversation.Key != 0)
            {
                conversation.Value.Remove(connectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<object> GetUserInfo()
        {
            if (long.TryParse(Context.UserIdentifier, out long userId))
            {
                var user = await _context.user.FindAsync(userId);
                return new
                {
                    user.UserName,
                    user.Role
                };
            }
            return null;
        }

        /// <summary>
        /// Thoát khỏi cuộc trò chuyện hiện tại và tham gia cuộc trò chuyện mới
        /// </summary>
        /// <param name="conversationId"></param>
        public void LeaveAndJoinGroup(int conversationId)
        {
            var connectionId = Context.ConnectionId;
            var conversation = UserConversations.FirstOrDefault(x => x.Value.Contains(connectionId));
            if (conversation.Key != 0)
            {
                conversation.Value.Remove(connectionId);
            }

            if (UserConversations.TryGetValue(conversationId, out var connectionIds))
            {
                connectionIds.Add(connectionId);
            }
            else
            {
                UserConversations[conversationId] = [connectionId];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task SendMessage(int conversationId, string content)
        {
            if (!int.TryParse(Context.UserIdentifier, out int senderId)) return;

            var conversation = await GetOrCreateConversationId(senderId, conversationId);
            var message = new MessageModel
            {
                ConversationId = conversation.ConversationId,
                SenderId = senderId,
                Content = content,
                IsSeen = false,
                IsAdmin = Context.User.IsAdmin()
            };

            await _context.messages.AddAsync(message);

            // Cập nhật LastMessageTime cho cuộc trò chuyện
            conversation.LastMessageTime = DateTime.UtcNow;
            conversation.LastMessage = content;

            await _context.SaveChangesAsync();
            var messageResponse = new
            {
                message.MessageId,
                message.SenderId,
                message.Content,
                message.IsSeen,
                message.IsAdmin,
                message.Timestamp,
                conversation.ConversationId
            };

            if (!Context.User.IsAdmin())
            {
                // lấy ra danh sách các admin có trong hệ thống
                var adminIds = await _context.Users
                    .Where(u => u.Role == "ADMIN")
                    .Select(u => u.Id)
                    .ToListAsync();

                // Lọc các adminId có trong UserConnections
                var connectedAdmins = UserConnections
                    .Where(kv => adminIds.Contains(kv.Key)) // Kiểm tra adminId có trong danh sách adminIds
                    .ToList(); // Chuyển thành danh sách để dễ duyệt qua

                await Clients.Clients(connectedAdmins.Select(kv => kv.Value)).SendAsync("ReceiveMessage", messageResponse);
                await Clients.Caller.SendAsync("ReceiveMessage", messageResponse);
            }
            else
            {
                if (UserConversations.TryGetValue(conversation.ConversationId, out var connectionIds))
                {
                    await Clients.Clients(connectionIds).SendAsync("ReceiveMessage", messageResponse);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task MarkMessageAsSeen(int conversationId, int messageId)
        {
            var message = await _context.messages.FindAsync(messageId);
            if (message != null)
            {
                message.IsSeen = true;
                await _context.SaveChangesAsync();

                if (UserConnections.TryGetValue(message.SenderId, out var senderConnectionId))
                {
                    await Clients.Client(senderConnectionId).SendAsync("MessageSeen", messageId);
                }
            }
        }

        /// <summary>
        /// Lấy hoặc tạo cuộc trò chuyện
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<ConversationModel> GetOrCreateConversationId(int userId, int conversationId)
        {
            if (!Context.User.IsAdmin())
            {
                var connectionId = Context.ConnectionId;
                conversationId = UserConversations.FirstOrDefault(x => x.Value.Contains(connectionId)).Key;
            }

            var conversation = await _context.conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
            {
                long adminId = 0;
                if (Context.User.IsAdmin())
                {
                    if (!long.TryParse(Context.UserIdentifier, out adminId)) return null;
                }
                else
                {
                    var firstAdmin = await _context.Users.FirstOrDefaultAsync(x => x.Role == "ADMIN")
                        ?? throw new Exception("No admin found in the database");
                    adminId = firstAdmin.Id;
                }

                conversation = new ConversationModel
                {
                    UserId = userId,
                    AdminId = adminId,
                    LastMessageTime = DateTime.UtcNow,
                    LastMessage = string.Empty
                };

                await _context.conversations.AddAsync(conversation);
                await _context.SaveChangesAsync();

                LeaveAndJoinGroup(conversation.ConversationId);

                if (!Context.User.IsAdmin())
                {
                    // lấy ra danh sách các admin có trong hệ thống
                    var adminIds = await _context.Users
                        .Where(u => u.Role == "ADMIN")
                        .Select(u => u.Id)
                        .ToListAsync();

                    // Lọc các adminId có trong UserConnections
                    var connectedAdmins = UserConnections
                        .Where(kv => adminIds.Contains(kv.Key)) // Kiểm tra adminId có trong danh sách adminIds
                        .ToList(); // Chuyển thành danh sách để dễ duyệt qua

                    // include thêm thông tin của user để hiển thị
                    await _context.Entry(conversation).Reference(c => c.User).LoadAsync();

                    // Gửi thông báo cho các admin đang online
                    await Clients.Clients(connectedAdmins.Select(kv => kv.Value)).SendAsync("ReceiveConversations", new[]
                    {
                        new
                        {
                            conversation.ConversationId,
                            conversation.UserId,
                            conversation.User.UserName,
                            conversation.User.Email,
                            conversation.LastMessageTime,
                            conversation.LastMessage
                        }
                    }, true);
                }
            }

            return conversation;
        }

        /// <summary>
        /// Lấy danh sách các cuộc trò chuyện của người dùng
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task GetConversations(string keyword = "")
        {
            var conversations = _context.conversations.AsQueryable();
            if (!Context.User.IsAdmin())
            {
                if (!long.TryParse(Context.UserIdentifier, out long userId)) return;
                conversations = conversations.Where(c => c.UserId == userId || c.AdminId == userId);
            }
            conversations = conversations.Include(c => c.User);
            conversations = conversations.OrderByDescending(c => c.LastMessageTime);

            if (!string.IsNullOrEmpty(keyword))
            {
                conversations = conversations.Where(c => c.User.UserName.Trim().ToLower().Contains(keyword.Trim().ToLower())
                     || c.User.Email.Trim().ToLower().Contains(keyword.Trim().ToLower()));
            }

            var conversationsRs = await conversations
                .Select(c => new
                {
                    c.ConversationId,
                    c.UserId,
                    c.User.UserName,
                    c.User.Email,
                    c.LastMessageTime,
                    c.LastMessage
                })
                .ToListAsync();

            if (!Context.User.IsAdmin() && conversationsRs.Count != 0)
            {
                var connectionId = Context.ConnectionId;
                var latestConversation = conversationsRs.First();
                if (UserConversations.TryGetValue(latestConversation.ConversationId, out var connectionIds))
                {
                    connectionIds.Add(connectionId);
                }
                else
                {
                    UserConversations[latestConversation.ConversationId] = [connectionId];
                }

                await GetMessages();
            }

            await Clients.Caller.SendAsync("ReceiveConversations", conversationsRs);
        }

        /// <summary>
        /// Lấy danh sách tin nhắn trong cuộc trò chuyện
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        public async Task GetMessages(int conversationId = 0, int page = 1)
        {
            if (!Context.User.IsAdmin())
            {
                var connectionId = Context.ConnectionId;
                conversationId = UserConversations.FirstOrDefault(x => x.Value.Contains(connectionId)).Key;
            }

            const int pageSize = 30;
            var messages = await _context.messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.MessageId,
                    m.SenderId,
                    m.Content,
                    m.IsSeen,
                    m.IsAdmin,
                    m.Timestamp,
                    m.ConversationId
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("ReceiveMessages", messages);
        }
    }
}