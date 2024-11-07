$(document).ready(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    let selectedConversationId = 0,
        currentUserInfo = null,
        currentPage = 1,
        isLoadingMessages = false,
        hasMoreMessages = true,
        isFirstLoad = true,
        isSearching = false;

    // Khởi tạo kết nối
    connection.start().then(async function () {
        console.log("Kết nối SignalR thành công!");
        await fetchUserInfo(); // Lấy thông tin người dùng
        loadConversations();  // Tải danh sách người nhắn sau khi kết nối thành công
    }).catch(function (err) {
        console.error("Lỗi kết nối SignalR:", err);
    });

    // Lấy danh sách người nhắn
    function loadConversations() {
        connection.invoke("GetConversations", "").catch(function (err) {
            console.error("Lỗi khi gọi GetConversations:", err);
        });
    }

    // Lấy thông tin người dùng
    async function fetchUserInfo() {
        var info = await connection.invoke("GetUserInfo");
        currentUserInfo = info;
    }

    // Nhận danh sách người nhắn và hiển thị
    connection.on("ReceiveConversations", function (conversations, isNewUserMessage = false) {
        // nếu đang tìm kiếm mà có người nhắn mới thì không hiển thị
        if (isSearching && isNewUserMessage) return;

        if (!isNewUserMessage) $(".inbox_chat").empty();
        if (!Array.isArray(conversations)) {
            conversations = [conversations];
        }

        $.each(conversations, function (index, conv) {
            let { dateOnly, timeOnly, fullDateTime } = formatDate(conv.lastMessageTime)
            let content = `
                <div class="chat_list" data-conversation-id="${conv.conversationId}" data-user-id="${conv.userId}">
                    <div class="chat_people">
                        <div class="chat_img"><img src="https://ptetutorials.com/images/user-profile.png" alt="user"></div>
                        <div class="chat_ib">
                            <h5>${conv.userName} <span class="chat_date">${fullDateTime}</span></h5>
                            <p class="last-message">${conv.lastMessage}</p>
                        </div>
                    </div>
                </div>`
                
            if (!isNewUserMessage){
                $(".inbox_chat").append(content);
            } else {
                $(".inbox_chat").prepend(content);
            }
        });
    });

    // Xử lý khi nhấn vào một người nhắn để tải tin nhắn của họ
    $(document).on("click", ".chat_list", function () {
        const conversationId = $(this).data("conversation-id");
        if (conversationId !== selectedConversationId) {
            $(".msg_history").children().not(".scroll-down-container").remove();
            selectedConversationId = conversationId;
            connection.invoke("LeaveAndJoinGroup", conversationId).catch(function (err) {
                console.error("Lỗi khi tham gia nhóm:", err);
            });

            currentPage = 1;
            hasMoreMessages = true;
            isFirstLoad = true;
            loadMessages(conversationId);

            // remove active_chat class
            $(".chat_list").removeClass("active_chat");
            $(this).addClass("active_chat");
        }
    });

    // Hàm chuyển đổi ngày tháng
    function formatDate(date) {
        if (typeof date === 'string') date = new Date(date);

        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        var day = date.getDate();
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var d = hours >= 12 ? 'PM' : 'AM';

        // Correction
        if (hours > 12) hours = hours % 12;

        if (minutes < 10) minutes = '0' + minutes;

        // Result
        var dateOnly = `${day}/${month}/${year}`;
        var timeOnly = `${hours}:${minutes} ${d}`;
        var fullDateTime = `${dateOnly} ${timeOnly}`;

        return { dateOnly, timeOnly, fullDateTime };
    }

    // Lấy và hiển thị tin nhắn của một cuộc trò chuyện
    async function loadMessages(conversationId) {
        if (isLoadingMessages || !hasMoreMessages) return;
        isLoadingMessages = true;
        
        try {
            await connection.invoke("GetMessages", conversationId, currentPage);
        } catch (err) {
            console.error("Lỗi khi gọi GetMessages:", err);
            isLoadingMessages = false;
        }
    }

    // Xử lý sự kiện cuộn để tải thêm tin nhắn 
    $('.msg_history').on('scroll', async function () {
        $(".scroll-down-container").hide();
        if ($(this).scrollTop() === 0 && hasMoreMessages) {
            await loadMessages(selectedConversationId);
        }
    });

    // Xử lý khi người dùng nhấn nút cuộn xuống
    $(".scroll-down-container").on("click", function () {
        $(".msg_history").animate({ scrollTop: $(".msg_history")[0].scrollHeight }, 1000);
    });

    // Nhận tin nhắn và hiển thị trong phần lịch sử trò chuyện
    connection.on("ReceiveMessages", function (messages) {
        // $(".msg_history").empty();
        isLoadingMessages = false;
        if (messages.length === 0) {
            hasMoreMessages = false;
            return;
        }

        if (!selectedConversationId) {
            selectedConversationId = messages[0].conversationId;
        }

        const container = $(".msg_history");
        const oldScrollHeight = container[0].scrollHeight;

        $.each(messages, function (index, msg) {
            bindingMessage(msg);
        });

        currentPage++;
        // cuộn xuống cuối
        if (isFirstLoad) {
            isFirstLoad = false;
            $(".msg_history").animate({ scrollTop: $(".msg_history")[0].scrollHeight }, 1000);
        } else {
            // Điều chỉnh vị trí scroll sau khi thêm tin nhắn
            const newScrollHeight = container[0].scrollHeight;
            container.scrollTop(newScrollHeight - oldScrollHeight);
        }
    });

    // Hiển thị tin nhắn
    function bindingMessage(msg, isNewUserMessage = false) {
        let { dateOnly, timeOnly, fullDateTime } = formatDate(msg.timestamp);
        let conversationId = msg.conversationId;

        if (isNewUserMessage){
            let conversation = $(`.chat_list[data-conversation-id=${conversationId}]`);
            if (conversation.length > 0) {
                let lastMessage = msg.content;
                let lastMessageTime = msg.timestamp;
                conversation.find(".last-message").text(lastMessage);
                conversation.find(".chat_date").text(fullDateTime);

                conversation.remove();
                $(".inbox_chat").prepend(conversation);
            }
        }
        if (currentUserInfo.role === "ADMIN" && conversationId !== selectedConversationId) return;

        let isAdmin = currentUserInfo.role === "ADMIN" ? msg.isAdmin : !msg.isAdmin;
        const messageHtml = isAdmin
            ? `
                <div class="outgoing_msg" title="${fullDateTime}">
                    <div class="sent_msg">
                        <p>${msg.content}</p>
                        <!-- <span class="time_date">${fullDateTime}</span> -->
                    </div>
                </div>
            `
            : `
                <div class="incoming_msg" title="${fullDateTime}">
                    <div class="received_msg">
                        <div class="incoming_msg_img"><img src="https://ptetutorials.com/images/user-profile.png" alt="user"></div>
                        <div class="received_withd_msg">
                            <p>${msg.content}</p>
                            <!-- <span class="time_date">${fullDateTime}</span>
                        </div>
                    </div>
                </div>
            `;

        if (isNewUserMessage) {
            $(".msg_history").append(messageHtml);

            // nếu thanh scroll không ở cuối thì hiển thị một nút để cuộn xuống
            if ($(".msg_history").scrollTop() + $(".msg_history").innerHeight() < $(".msg_history")[0].scrollHeight) {
                setTimeout(function () {
                    $(".scroll-down-container").show();
                }, 1100);
            }
        } else {
            $(".msg_history").prepend(messageHtml);
        }
    }

    // Gửi tin nhắn
    $(".msg_send_btn").on("click", function () {
        sendMessage();
    });

    // Gửi tin nhắn khi nhấn Enter
    $(".write_msg").on("keypress", function (e) {
        if (e.which === 13) {
            sendMessage();
        }
    });

    // Gửi tin nhắn
    function sendMessage() {
        const content = $(".write_msg").val().trim();
        if (content) {
            connection.invoke("SendMessage", selectedConversationId, content).catch(function (err) {
                console.error("Lỗi khi gửi tin nhắn:", err);
            });
            $(".write_msg").val("");
        }

        $(".msg_history").animate({ scrollTop: $(".msg_history")[0].scrollHeight }, 1000);
    }

    // Nhận tin nhắn mới
    connection.on("ReceiveMessage", function (content) {
        bindingMessage(content, true);
    });

    // xử lý khi gõ .search-bar có sử dụng debounce
    let timeout = null;
    $(".search-bar").on("input", function () {
        const keyword = $(".search-bar").val().trim();
        isSearching = keyword.length > 0;
        clearTimeout(timeout);
        timeout = setTimeout(function () {
            connection.invoke("GetConversations", keyword).catch(function (err) {
                console.error("Lỗi khi tìm kiếm:", err);
            });
        }, 500);
    });
});