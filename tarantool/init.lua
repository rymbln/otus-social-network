#!/usr/bin/env tarantool

-- Add Taranrocks pathes. https://github.com/rtsisyk/taranrocks/blob/master/README.md
local home = os.getenv("HOME")
package.path = [[/usr/local/share/tarantool/lua/?/init.lua;]]..package.path
package.path = [[/usr/local/share/tarantool/lua/?.lua;]]..package.path
package.path = home..[[/.tarantool/share/tarantool/lua/?/init.lua;]]..package.path
package.path = home..[[/.tarantool/share/tarantool/lua/?.lua;]]..package.path
package.cpath = [[/usr/local/lib/tarantool/lua/?.so;]]..package.cpath
package.cpath = home..[[/.tarantool/lib/tarantool/lua/?.so;]]..package.cpath

local log = require('log')
local uuid = require("uuid")

box.cfg
{
    pid_file = nil,
    background = false,
    log_level = 5
}

local function init()
    box.schema.user.create('operator', {password = '123123', if_not_exists = true})
    box.schema.user.grant('operator', 'read,write,execute', 'universe', nil, { if_not_exists = true })

    ----------
    --- POSTS
    ----------

    posts = box.schema.create_space('posts', { if_not_exists = true })

    -- Add fields to the space
    posts:format({
        {name = 'id', type = 'string'},
        {name = 'userid', type = 'string'},
        {name = 'postid', type = 'string'},
        {name = 'numberid', type = 'number'},
        {name = 'content', type = 'string'},
    })
    log.info(posts.name .. " space was created.")

     -- Create posts indexes
     posts:create_index('primary', {
        if_not_exists = true,
        type = 'HASH',
        unique = true,
        parts = { { field = 'id', type = 'string'}}
    })

    log.info("primary index created.")

    posts:create_index('secondary_user', {
        if_not_exists = true,
        unique = false,
        parts = {
            { field = 'userid', type = 'string'}
        }
    })

    posts:create_index('secondary_post', {
        if_not_exists = true,
        unique = false,
        parts = {
            { field = 'postid', type = 'string'}
        }
    })

    posts:create_index('secondary_user_number', {
        if_not_exists = true,
        unique = false,
        parts = {
            { field = 'userid', type = 'string'},
            {field = 'numberid', type = 'number'}
        }
    })
    log.info("secondary index created.")

    ----------
    --- POST SOCKET
    ----------

    usersockets = box.schema.create_space('usersockets', { if_not_exists = true })
    usersockets:format({
        {name = 'id', type = 'string'},
        {name = 'userid', type = 'string'},
        {name = 'connectionid', type = 'string'}
    })
    log.info(usersockets.name .. " space was created.")



    -- Create sockets indexes

    usersockets:create_index('usersockets_idx_primary', {
        if_not_exists = true,
        type = 'HASH',
        unique = true,
        parts = { { field = 'id', type = 'string'}}
    })
    usersockets:create_index('usersockets_idx_userid', {
        if_not_exists = true,
        unique = false,
        parts = { { field = 'userid', type = 'string'} }
    })
    usersockets:create_index('usersockets_idx_connectionid', {
        if_not_exists = true,
        unique = false,
        parts = { { field = 'connectionid', type = 'string'} }
    })
    log.info("usersockets index created.")

    ----------
    --- CHAT SOCKET
    ----------
    chatsockets = box.schema.create_space('chatsockets', { if_not_exists = true })
    chatsockets:format({
        {name = 'id', type = 'string'},
        {name = 'userid', type = 'string'},
        {name = 'connectionid', type = 'string'}
    })
    log.info(chatsockets.name .. " space was created.")

    chatsockets:create_index('chatsockets_idx_primary', {
        if_not_exists = true, type = 'HASH', unique = true, parts = { { field = 'id', type = 'string'}}
    })
    chatsockets:create_index('chatsockets_idx_userid', {
        if_not_exists = true, unique = false, parts = { { field = 'userid', type = 'string'} }
    })
    chatsockets:create_index('chatsockets_idx_connectionid', {
        if_not_exists = true, unique = false, parts = { { field = 'connectionid', type = 'string'} }
    })
    log.info("chatsockets index created.")

    ---------
    --- CHAT
    ---------
    -- Create the space
    chats = box.schema.create_space('chats', { if_not_exists = true })
    log.info("chats created.")
    -- Define the format of the tuple
    local format = {
        {name = 'id', type = 'string'},
        {name = 'name', type = 'string'},
    }
    -- Create the field map
    local field_map = {}
    for i, field in ipairs(format) do
        field_map[field.name] = i
    end
    chats:format(format)
    log.info("chats:format")
    -- Create an index on the ID field
    chats:create_index('chat_primary', {
        if_not_exists = true, unique = true, type = 'tree',
        parts = {field_map.id}
    })
    log.info("chats:index created")
    ---------
    --- CHAT-USERS
    ---------
    chats_and_users = box.schema.create_space('chats_and_users', { if_not_exists = true })
    log.info("chats_and_users:created.")
    local format = {
        {name = 'id', type = 'string'},
        {name = 'chatid', type = 'string'},
        {name = 'userid', type = 'string'},
    }
    -- Create the field map
    local field_map = {}
    for i, field in ipairs(format) do
        field_map[field.name] = i
    end
    chats_and_users:format(format)
    log.info("chats_and_users:format")
    -- Create an index on the ID field
    chats_and_users:create_index('chats_and_users_primary', {
        if_not_exists = true, unique = true, type = 'tree',
        parts = {field_map.id}
    })
    chats_and_users:create_index('chats_and_users_chatid_idx', {if_not_exists = true, unique = false, parts = {field_map.chatid}})
    chats_and_users:create_index('chats_and_users_userid_idx', {if_not_exists = true, unique = false, parts = {field_map.userid}})

    log.info("chats_and_users:index created")
    ---------
    --- MESSAGES
    ---------
    -- Create the space
    messages = box.schema.create_space('messages', { if_not_exists = true })
    log.info("messages:created")
    -- Define the format of the tuple
    local format = {
        {name = 'id', type = 'string'},
        {name = 'chatid', type = 'string'},
        {name = 'userid', type = 'string'},
        {name = 'messagetext', type = 'string'},
        {name = 'timestamp', type = 'string'}
    }
    -- Create the field map
    local field_map = {}
    for i, field in ipairs(format) do
        field_map[field.name] = i
    end
    -- Set the format of the space
    messages:format(format)
    log.info("messages:formatted")
     -- Create an index on the ID field
    messages:create_index('message_primary', { if_not_exists = true, unique = true, type = 'tree', parts = {field_map.id} })
    log.info("message_primary:created")
     -- Create an index on the userIds field
    messages:create_index('message_chatid_secondary', {if_not_exists = true, unique = false, parts = {field_map.chatid}})
    log.info("message_primary:created")

    ---------
    --- DIALOG MESSAGES
    ---------
    -- Create the space
    dialog_messages = box.schema.create_space('dialog_messages', { if_not_exists = true })
    log.info("dialog_messages:created")
    -- Define the format of the tuple
    local format = {
        {name = 'id', type = 'string'},
        {name = 'fromid', type = 'string'},
        {name = 'toid', type = 'string'},
        {name = 'isnew', type = 'boolean'},
        {name = 'messagetext', type = 'string'},
        {name = 'timestamp', type = 'string'}
    }
    -- Create the field map
    local field_map = {}
    for i, field in ipairs(format) do
        field_map[field.name] = i
    end
    -- Set the format of the space
    dialog_messages:format(format)
    log.info("dialog_messages:formatted")
     -- Create an index on the ID field
    dialog_messages:create_index('dialog_messages_primary', { if_not_exists = true, unique = true, type = 'tree', parts = {field_map.id} })
    log.info("dialog_messages_primary:created")
     -- Create an index on the userIds field
    dialog_messages:create_index('dialog_messages_secondary', {if_not_exists = true, unique = false, parts = {field_map.fromid, field_map.toid}})
    dialog_messages:create_index('dialog_messages_unread', {if_not_exists = true, unique = false, parts = {field_map.fromid, field_map.toid, field_map.isnew}})
    log.info("dialog_messages_secondary:created")

    ---------
    --- COUNTERS
    ---------
    counters = box.schema.create_space('counters', { if_not_exists = true })
    log.info("counters:created")
    -- Define the format of the tuple
    local format = {
        {name = 'id', type = 'string'},
        {name = 'fromid', type = 'string'},
        {name = 'toid', type = 'string'},
        {name = 'unread', type = 'number'}
    }
    -- Create the field map
    local field_map = {}
    for i, field in ipairs(format) do
        field_map[field.name] = i
    end
    -- Create indexes
    counters:create_index('counters_primary', { if_not_exists = true, unique = true, type = 'tree', parts = {field_map.id} })
    log.info("counters_primary:created")
     -- Create an index on the userIds field
    counters:create_index('counters_secondary', {if_not_exists = true, unique = false, parts = {field_map.fromid, field_map.toid}})
    -- Create an index on the userIds field
    counters:create_index('counters_from_secondary', {if_not_exists = true, unique = false, parts = {field_map.toid}})
    log.info("counters_secondary:created")

end

function set_counters(fromid, toid, val)
    log.info("set_counters: " .. fromid .. " - " .. toid .. " - " .. val)
    local space = box.space.counters
    local index = space.index.counters_secondary

    if index:count({fromid, toid}) > 0 then
        local data = index:select({fromid, toid})
        -- Update value
        for idx, item in ipairs(data) do
            space:update(item[1], {{'=', 4, val}})
        end
    else
        -- Create value
        space:insert{uuid.str(), fromid, toid, val}
    end
    data = index:select({fromid, toid})
    log.info("set_counters:")
    log.info(data)
    return data
end

function get_counters(fromid, toid)
    local space = box.space.counters
    local index = space.index.counters_secondary
    local data = {}
    local id = 0

    local data1 = index:select({fromid, toid})
    if data1 ~= nil then
        for idx, item in ipairs(data1) do
            id = id + 1
            data[id] = item
        end
    end
    return data
end

function get_counters_all(toid)
    local space = box.space.counters
    local index = space.index.counters_from_secondary
    local data = index:select{toid}
    log.info("get_counters_all:")
    log.info(data)
    return data
end

function delete_usersockets_by_user(userid)
    local space = box.space['usersockets']
    local index = space.index['usersockets_idx_userid']
    -- Select tuples by index value
    local result = index:select(userid)

    for _,tuple in pairs(result) do
        tuple:delete()
    end
end

function delete_usersockets_by_connection(connectionid)
    local space = box.space['usersockets']
    local index = space.index['usersockets_idx_connectionid']
    -- Select tuples by index value
    local result = index:select(userid)

    for _,tuple in pairs(result) do
        tuple:delete()
    end
end

function update_post_idx(userid)
    local space = box.space['posts']
    local index = space.index['secondary_user']
    -- Select tuples by index value
    local result = index:select(userid)

    -- Update a field value in each selected tuple
    for k,v in pairs(result) do
        space:update(v[1], {{'+', 4, 1}})
    end
end

function delete_user_posts(userid)
    local space = box.space['posts']
    local index = space.index['secondary_user']
    local result = index:select(userid)

    for _,tuple in pairs(result) do
        tuple:delete()
    end
end

function delete_post(postid)
    local space = box.space['posts']
    local index = space.index['secondary_post']
    local result = index:select(postid)
    -- Iterate over the tuples in the index
    for _,tuple in pairs(result) do
        tuple:delete()
    end
end

function get_chat_for_user(chatid)
    log.info("get_chat_for_user:" .. chatid)
    local chats_and_users = box.space['chats_and_users']
    local chats_and_users_idx_user = chats_and_users.index.chats_and_users_userid_idx
    local chats_and_users_idx_chat = chats_and_users.index.chats_and_users_chatid_idx
    local chats = box.space.chats
    local result_chat_users = chats_and_users_idx_chat:select(chatid)
    local item_chat = chats:get(chatid)
    local userids = {}
    if result_chat_users ~= nil then
        for idx, val in ipairs(result_chat_users) do
            table.insert(userids, val.userid)
        end
    end
    tuple_item = box.tuple.new{item_chat.id, item_chat.name, userids}
    log.info(tuple_item)
    return tuple_item
end

function get_chats_for_user(userid)
    log.info("get_chats_for_user:" .. userid)
    local chats_and_users = box.space['chats_and_users']
    local chats_and_users_idx_user = chats_and_users.index.chats_and_users_userid_idx
    local chats_and_users_idx_chat = chats_and_users.index.chats_and_users_chatid_idx
    local chats = box.space.chats
    local results = {}
    local result_chat_users = chats_and_users_idx_user:select(userid)
    if result_chat_users ~= nil then
        for idx, val in ipairs(result_chat_users) do
            local item_chat = chats:get(val.chatid)
            local item_chat_users = chats_and_users_idx_chat:select(val.chatid)
            local userids = {}
            if item_chat_users ~= nil then
                for _, item_chat_user in ipairs(item_chat_users) do
                    table.insert(userids, item_chat_user.userid)
                end
            end
            if item_chat ~= nil then
                tuple_item = box.tuple.new{item_chat.id, item_chat.name, userids}
                results[idx] = tuple_item
            end
        end
    end
    log.info(results)
    return results
end

function create_chat(id, name, user_ids)
    local chats = box.space.chats
    local chats_and_users = box.space.chats_and_users
    chats:insert({ id, name})

    for index, user_id in ipairs(user_ids) do
        chats_and_users:insert({id .. "|" .. user_id, id, user_id} )
    end
end

function delete_chat(id)
    -- Delete chat
    local chats = box.space.chats
    local chat_index = chats.index.chat_primary
    local chatresult = chat_index:select(id)
    for _,tuple in pairs(chatresult) do
        chats:delete(tuple[1])
    end
    -- Delete chats and users
    local chats_and_users = box.space.chats_and_users
    local index = space.index.chats_and_users_chatid_idx
    local result = index:select(id)
    for _,tuple in pairs(result) do
        chats_and_users:delete(tuple[1])
    end
    -- Delete messages for chat
    delete_messages_by_chatid(id)
end

function get_messages_for_chat(chatid)
    -- Get the space and index objects
    local space = box.space.messages
    local index = space.index.message_chatid_secondary

    -- Perform a select query using the index
    local result = index:select(chatid)

    -- Return the selected items
    return result
end

function get_messages_for_dialog(fromid, toid)
    local space = box.space.dialog_messages
    local index = space.index.dialog_messages_secondary
    local data = {}
    local id = 0

    local data1 = index:select({fromid, toid})
    if data1 ~= nil then
        for idx, item in ipairs(data1) do
            id = id + 1
            data[id] = item
        end
    end

    local data2 = index:select({toid, fromid})
    if data2 ~= nil then
        for idx, item in ipairs(data2) do
            id = id + 1
            data[id] = item
        end
    end

    return data
end

function create_message(id, chatid, userid, messagetext, timestamp)
    -- Get the space object
    local space = box.space.messages

    -- Insert a new tuple with the provided values
    space:insert{id, chatid, userid, messagetext, timestamp}
end

function create_dialog_message(id, fromid, toid, messagetext, timestamp)
    -- Get the space object
    local space = box.space.dialog_messages

    -- Insert a new tuple with the provided values
    space:insert{id, fromid, toid, true, messagetext, timestamp}

    set_dialog_message_unread_count(fromid, toid)
end

function set_dialog_message_unread_count(fromid, toid)
    local space = box.space.dialog_messages
    local index = space.index.dialog_messages_unread
    local data = index:count({fromid, toid, true})
    log.info("set_dialog_message_unread_count:")
    log.info(data)
    if data ~= nil then
        set_counters(fromid, toid, data)
    end
end

function read_dialog_message(id)
    -- Get the space object
    local space = box.space.dialog_messages
    local index = space.index.dialog_messages_primary

    -- Insert a new tuple with the provided values
    local data = index:select({id})
    if data ~= nil then
        -- Update value
        for idx, item in ipairs(data) do
            space:update(v[1], {{'+', 5, false}})
        end
    end
end

function delete_message(id)
    log.info("delete_message:"..id)
    -- Get the space object
    local space = box.space.messages
    local index = space.index.message_primary
    -- Find the tuple with the specified ID and delete it
    local result =  index:select(id)
    for _,tuple in pairs(result) do
        log.info(tuple)
        space:delete(tuple[1])
    end
end

function delete_messages_by_chatid(chatid)
    -- Get the space and index objects
    local space = box.space.messages
    local index = space.index.message_chatid_secondary
    local result = index:select(chatid)
    -- Iterate over the tuples in the index
    for _,tuple in pairs(result) do
        space:delete(tuple[1])
    end
end

function add_chatsocket(id, userid, connectionid)
    local space = box.space.chatsockets
    -- Insert a new tuple with the provided values
    space:insert{id, userid, connectionid}
end

function delete_chatsockets_by_user(userid)
    local space = box.space.chatsockets
    local index = space.index.chatsockets_idx_userid
    -- Select tuples by index value
    local result = index:select(userid)
    for _,tuple in pairs(result) do
        space:delete(tuple[1])
    end
end

function delete_chatsockets_by_connection(connectionid)
    local space = box.space.chatsockets
    local index = space.index.chatsockets_idx_connectionid
    -- Select tuples by index value
    local result = index:select(userid)
    for _,tuple in pairs(result) do
        space:delete(tuple[1])
    end
end

function get_chatsockets(userid)
    local space = box.space.chatsockets
    local index = space.index.chatsockets_idx_userid
    -- Select tuples by index value
    local result = index:select(userid)

    return result
end

function data()
    -- local chats = box.space.chats
    -- local chats_and_users = box.space.chats_and_users
    -- local messages = box.space.messages
    -- local dialog_messages = box.space.dialog_messages

    -- -- Define the values for the new chat item
    -- local id = "b65ec3ae-cc92-4ad2-a6a6-1340503a648e"
    -- local name = "Chat Name"
    -- local user_ids = { "b2428a06-ee2a-40b8-94f4-69cd3c85a2e0", "3cc744e5-ec95-4926-9a64-aba219819337" }

    -- if chats:len() == 0 then
    --     -- Insert the new chat item
    --     chat_id_str = "b65ec3ae-cc92-4ad2-a6a6-1340503a648e"
    --     user_ids_str = {"b2428a06-ee2a-40b8-94f4-69cd3c85a2e0", "3cc744e5-ec95-4926-9a64-aba219819337"}
    --     chats:insert({ id, name })
    --     -- uuid.str()
    --     chats_and_users:insert(
    --         { "1", chat_id_str, user_ids_str[1]})
    --     chats_and_users:insert(
    --         { "2", chat_id_str, user_ids_str[2] }
    --     )
    --     log.info("chat created")
    -- end
    -- if messages:len() == 0 then
    --     -- insert messages
    --     for i = 1,1000,1 do
    --         for j = 1,2,1 do
    --             messages:insert({uuid.str(), chat_id_str, user_ids_str[j],
    --             'some interesting message number ' .. i .. ' from ' .. user_ids_str[j], os.date("!%Y-%m-%dT%H:%M:%SZ") })
    --         end
    --     end
    --     log.info("messages created")
    -- end

    -- if dialog_messages:len() == 0 then
    --     -- insert messages
    --     for i = 1,1000,1 do
    --         dialog_messages:insert({uuid.str(), user_ids_str[1], user_ids_str[2],
    --         'some interesting message number ' .. i .. ' from ' .. user_ids_str[1], os.date("!%Y-%m-%dT%H:%M:%SZ") })
    --         dialog_messages:insert({uuid.str(), user_ids_str[2], user_ids_str[1],
    --         'some interesting message number ' .. i .. ' from ' .. user_ids_str[1], os.date("!%Y-%m-%dT%H:%M:%SZ") })
    --     end
    --     log.info("dialog_messages created")
    -- end

    -- local status, result = pcall(function()
    --     get_chats_for_user("3cc744e5-ec95-4926-9a64-aba219819337")
    -- end)

    -- print(status, result)

    -- if status then
    --     -- Success: handle the result
    --     print(result)
    -- else
    --     -- Error: handle the error message
    --     print("Error occurred:", result)
    -- end
end

box.once('init', init)
box.once('data', data)