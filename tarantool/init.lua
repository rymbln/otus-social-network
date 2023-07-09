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

box.cfg
{
    pid_file = nil,
    background = false,
    log_level = 5
}

local function init()
    box.schema.user.create('operator', {password = '123123', if_not_exists = true})
    box.schema.user.grant('operator', 'read,write,execute', 'universe', nil, { if_not_exists = true })

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
    
    usersockets = box.schema.create_space('usersockets', { if_not_exists = true })
    usersockets:format({
        {name = 'id', type = 'string'},
        {name = 'userid', type = 'string'},
        {name = 'connectionid', type = 'string'}
    })
    log.info(usersockets.name .. " space was created.")

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

box.once('init', init)
