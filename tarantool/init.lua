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
        {name = 'datetime', type = 'number'},
        {name = 'content', type = 'string'},
    })

    log.info(posts.name .. " space was created.")

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

    posts:create_index('secondary_user_date', {
        if_not_exists = true,
        unique = false,
        parts = { 
            { field = 'userid', type = 'string'}, 
            {field = 'datetime', type = 'number'} 
        }
    })
    log.info("secondary index created.")
end

function update_post_idx(userId)
    local space = box.space['posts']
    local index = space.index['secondary_user']
    -- Select tuples by index value
    local result = index:select(index_value)

    -- Update a field value in each selected tuple
    for k,v in pairs(result) do
        space:update(v[1], {{'+', 3, 1}}) 
    end
end

function delete_posts(userId) 
    local space = box.space['posts']
    local index = space.index['secondary_user']

    -- Iterate over the tuples in the index
    for _, tuple in index:pairs(userId) do
        tuple:delete()
    end
end 

box.once('init', init)
