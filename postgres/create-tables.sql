CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- public.account definition

CREATE TABLE IF NOT EXISTS public.account (
	id varchar NOT NULL,
	"password" varchar NOT NULL,
	CONSTRAINT account_pk PRIMARY KEY (id)
);

-- password qwerty
INSERT INTO public.account (id,"password") VALUES
	 ('3cc744e5-ec95-4926-9a64-aba219819337','AQAAAAEAACcQAAAAEDQ2X6yyV9TRUjASPws3IV3lx2Tdt2bEbu4k3oWI/ZFFkNy4HASbnGrASliO8mMDNQ=='),
	 ('3eefe556-a733-4ba9-ba28-3e55cc459a79','AQAAAAEAACcQAAAAEDQ2X6yyV9TRUjASPws3IV3lx2Tdt2bEbu4k3oWI/ZFFkNy4HASbnGrASliO8mMDNQ=='),
	 ('b2428a06-ee2a-40b8-94f4-69cd3c85a2e0','AQAAAAEAACcQAAAAEDQ2X6yyV9TRUjASPws3IV3lx2Tdt2bEbu4k3oWI/ZFFkNy4HASbnGrASliO8mMDNQ==');

-- public."user" definition

CREATE TABLE IF NOT EXISTS public."user" (
	id varchar NOT NULL,
	first_name varchar NOT NULL,
	second_name varchar NOT NULL,
	sex varchar NOT NULL,
	age int4 NOT NULL,
	city varchar NOT NULL,
	biography varchar NOT NULL,
	CONSTRAINT user_pk PRIMARY KEY (id)
);

INSERT INTO public."user" (id,first_name,second_name,sex,age,city,biography) VALUES
	 ('b2428a06-ee2a-40b8-94f4-69cd3c85a2e0','rymbln','rymbln','male',18,'smolensk','sdkjgklsjg'),
	 ('3eefe556-a733-4ba9-ba28-3e55cc459a79','test','2test','female',16,'moscow','dsgsg'),
	 ('3cc744e5-ec95-4926-9a64-aba219819337','qwerty','2qwerty','qwerty',89,'qwerty','qwerty');


-- public.posts definition

CREATE TABLE public.posts (
	id varchar NOT NULL,
	author_user_id varchar NOT NULL,
	post_text text NOT NULL,
	"timestamp" timestamp NOT NULL,
	CONSTRAINT post_pk PRIMARY KEY (id)
);
CREATE INDEX posts_author_user_id_idx ON public.posts USING btree (author_user_id, "timestamp");


-- public.posts foreign keys

ALTER TABLE public.posts ADD CONSTRAINT posts_fk FOREIGN KEY (author_user_id) REFERENCES public."user"(id);


INSERT INTO public.posts (id,author_user_id,post_text,"timestamp") VALUES
	 ('1d0e8d8b-a070-4698-a0f3-efb7784d5891','3cc744e5-ec95-4926-9a64-aba219819337','another part text','2023-06-17 21:49:58.325025'),
	 ('91467c42-b830-4bf0-a5bb-c7bfe2df112d','3eefe556-a733-4ba9-ba28-3e55cc459a79','new very interesting post','2023-07-01 18:07:26.816909'),
	 ('29018a63-eb27-4dc1-9265-dc862e3b5450','3eefe556-a733-4ba9-ba28-3e55cc459a79','sample post','2023-07-01 18:08:41.00406'),
	 ('b30b9976-d4f3-45aa-a27c-7675ba60269a','3cc744e5-ec95-4926-9a64-aba219819337','new sample post for tarantool','2023-07-02 10:03:21.768219'),
	 ('0dba8609-630d-4134-83d3-5d19dbacfcd8','3cc744e5-ec95-4926-9a64-aba219819337','another very interesting post','2023-07-02 12:53:29.378371'),
	 ('7ddd09a8-33f3-4ee6-a3ca-755627aa6e8f','3cc744e5-ec95-4926-9a64-aba219819337','special post to check tarantool','2023-07-02 13:31:00.581281'),
	 ('c131c27e-38d0-4e73-bd71-015561b19690','3cc744e5-ec95-4926-9a64-aba219819337','new post prom qwerty qwerty','2023-07-02 13:33:39.519496');


create or replace function lipsum( quantity_ integer ) returns character varying
    language plpgsql
    as $$
  declare
    words_       text[];
    returnValue_ text := '';
    random_      integer;
    ind_         integer;
  begin
  words_ := array['lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'a', 'ac', 'accumsan', 'ad', 'aenean', 'aliquam', 'aliquet', 'ante', 'aptent', 'arcu', 'at', 'auctor', 'augue', 'bibendum', 'blandit', 'class', 'commodo', 'condimentum', 'congue', 'consequat', 'conubia', 'convallis', 'cras', 'cubilia', 'cum', 'curabitur', 'curae', 'cursus', 'dapibus', 'diam', 'dictum', 'dictumst', 'dignissim', 'dis', 'donec', 'dui', 'duis', 'egestas', 'eget', 'eleifend', 'elementum', 'enim', 'erat', 'eros', 'est', 'et', 'etiam', 'eu', 'euismod', 'facilisi', 'facilisis', 'fames', 'faucibus', 'felis', 'fermentum', 'feugiat', 'fringilla', 'fusce', 'gravida', 'habitant', 'habitasse', 'hac', 'hendrerit', 'himenaeos', 'iaculis', 'id', 'imperdiet', 'in', 'inceptos', 'integer', 'interdum', 'justo', 'lacinia', 'lacus', 'laoreet', 'lectus', 'leo', 'libero', 'ligula', 'litora', 'lobortis', 'luctus', 'maecenas', 'magna', 'magnis', 'malesuada', 'massa', 'mattis', 'mauris', 'metus', 'mi', 'molestie', 'mollis', 'montes', 'morbi', 'mus', 'nam', 'nascetur', 'natoque', 'nec', 'neque', 'netus', 'nibh', 'nisi', 'nisl', 'non', 'nostra', 'nulla', 'nullam', 'nunc', 'odio', 'orci', 'ornare', 'parturient', 'pellentesque', 'penatibus', 'per', 'pharetra', 'phasellus', 'placerat', 'platea', 'porta', 'porttitor', 'posuere', 'potenti', 'praesent', 'pretium', 'primis', 'proin', 'pulvinar', 'purus', 'quam', 'quis', 'quisque', 'rhoncus', 'ridiculus', 'risus', 'rutrum', 'sagittis', 'sapien', 'scelerisque', 'sed', 'sem', 'semper', 'senectus', 'sociis', 'sociosqu', 'sodales', 'sollicitudin', 'suscipit', 'suspendisse', 'taciti', 'tellus', 'tempor', 'tempus', 'tincidunt', 'torquent', 'tortor', 'tristique', 'turpis', 'ullamcorper', 'ultrices', 'ultricies', 'urna', 'ut', 'varius', 'vehicula', 'vel', 'velit', 'venenatis', 'vestibulum', 'vitae', 'vivamus', 'viverra', 'volutpat', 'vulputate'];
    for ind_ in 1 .. quantity_ loop
      ind_ := ( random() * ( array_upper( words_, 1 ) - 1 ) )::integer + 1;
      returnValue_ := returnValue_ || ' ' || words_[ind_];
    end loop;
    return returnValue_;
  end;
$$;

INSERT INTO public.posts (id, author_user_id, post_text, "timestamp")
select uuid_generate_v4()::varchar as id,
'3cc744e5-ec95-4926-9a64-aba219819337' as author_id,
lipsum(trunc(random()*100)::int) as post_text,
timestamp '2023-06-10 20:00:00' +
       random() * (timestamp '2023-01-20 20:00:00' -
                   timestamp '2023-05-10 10:00:00') as timestamp_val
from generate_series(1,1000)
;

INSERT INTO public.posts (id, author_user_id, post_text, "timestamp")
select uuid_generate_v4()::varchar as id,
'b2428a06-ee2a-40b8-94f4-69cd3c85a2e0' as author_id,
lipsum(trunc(random()*100)::int) as post_text,
timestamp '2023-06-10 20:00:00' +
       random() * (timestamp '2023-01-20 20:00:00' -
                   timestamp '2023-05-10 10:00:00') as timestamp_val
from generate_series(1,1000)
;


INSERT INTO public.posts (id, author_user_id, post_text, "timestamp")
select uuid_generate_v4()::varchar as id,
'3eefe556-a733-4ba9-ba28-3e55cc459a79' as author_id,
lipsum(trunc(random()*100)::int) as post_text,
timestamp '2023-06-10 20:00:00' +
       random() * (timestamp '2023-01-20 20:00:00' -
                   timestamp '2023-05-10 10:00:00') as timestamp_val
from generate_series(1,1000)
;







-- Drop table

-- DROP TABLE public.friends;

CREATE TABLE public.friends (
	user_id varchar NOT NULL,
	friend_id varchar NOT NULL,
	CONSTRAINT friends_pk PRIMARY KEY (user_id, friend_id)
);


-- public.friends foreign keys

ALTER TABLE public.friends ADD CONSTRAINT friends_fk FOREIGN KEY (user_id) REFERENCES public."user"(id);
ALTER TABLE public.friends ADD CONSTRAINT friends_fk_1 FOREIGN KEY (friend_id) REFERENCES public."user"(id);

INSERT INTO public.friends (user_id,friend_id) VALUES
	 ('b2428a06-ee2a-40b8-94f4-69cd3c85a2e0','3eefe556-a733-4ba9-ba28-3e55cc459a79'),
	 ('3eefe556-a733-4ba9-ba28-3e55cc459a79','b2428a06-ee2a-40b8-94f4-69cd3c85a2e0'),
	 ('b2428a06-ee2a-40b8-94f4-69cd3c85a2e0','3cc744e5-ec95-4926-9a64-aba219819337'),
	 ('3cc744e5-ec95-4926-9a64-aba219819337','b2428a06-ee2a-40b8-94f4-69cd3c85a2e0'),
	 ('3cc744e5-ec95-4926-9a64-aba219819337','3eefe556-a733-4ba9-ba28-3e55cc459a79');


--- Chat

-- public.chats definition

-- Drop table

-- DROP TABLE public.chats;

CREATE TABLE public.chats (
	id varchar NOT NULL,
	"name" varchar NOT NULL,
	CONSTRAINT chats_pk PRIMARY KEY (id)
);

INSERT INTO public.chats
(id, "name")
VALUES('b65ec3ae-cc92-4ad2-a6a6-1340503a648e', 'Chat Name');

-- public.chat_user definition

-- Drop table

-- DROP TABLE public.chat_user;

CREATE TABLE public.chat_user (
	chat_id varchar NOT NULL,
	user_id varchar NOT NULL,
	CONSTRAINT chat_user_pk PRIMARY KEY (chat_id, user_id)
);


INSERT INTO public.chat_user
(chat_id, user_id)
VALUES('b65ec3ae-cc92-4ad2-a6a6-1340503a648e', 'b2428a06-ee2a-40b8-94f4-69cd3c85a2e0'),
('b65ec3ae-cc92-4ad2-a6a6-1340503a648e', '3cc744e5-ec95-4926-9a64-aba219819337');


-- public.chat_user foreign keys

ALTER TABLE public.chat_user ADD CONSTRAINT chat_user_fk FOREIGN KEY (chat_id) REFERENCES public.chats(id);
ALTER TABLE public.chat_user ADD CONSTRAINT chat_user_fk_1 FOREIGN KEY (user_id) REFERENCES public."user"(id);



-- public.messages definition

-- Drop table

-- DROP TABLE public.messages;

CREATE TABLE public.messages (
	id varchar NOT NULL,
	chat_id varchar NOT NULL,
	user_id varchar NOT NULL,
	message_text varchar NOT NULL,
	is_new boolean NOT NULL,
	"timestamp" timestamp NOT NULL,
	CONSTRAINT messages_pk PRIMARY KEY (id)
);


-- public.messages foreign keys

ALTER TABLE public.messages ADD CONSTRAINT messages_fk FOREIGN KEY (chat_id) REFERENCES public.chats(id);
ALTER TABLE public.messages ADD CONSTRAINT messages_fk_1 FOREIGN KEY (user_id) REFERENCES public."user"(id);


INSERT INTO public.messages
(id, chat_id, user_id, message_text, is_new, "timestamp")
select
uuid_generate_v4()::varchar as id,
'b65ec3ae-cc92-4ad2-a6a6-1340503a648e' as chat_id,
'b2428a06-ee2a-40b8-94f4-69cd3c85a2e0' as user_id,
lipsum(trunc(random()*100)::int) as message_text,
false as is_new,
timestamp '2023-06-10 20:00:00' +
       random() * (timestamp '2023-01-20 20:00:00' -
                   timestamp '2023-05-10 10:00:00') as timestamp_val
from generate_series(1,500)
;
INSERT INTO public.messages
(id, chat_id, user_id, message_text, is_new, "timestamp")
select
uuid_generate_v4()::varchar as id,
'b65ec3ae-cc92-4ad2-a6a6-1340503a648e' as chat_id,
'3cc744e5-ec95-4926-9a64-aba219819337' as user_id,
lipsum(trunc(random()*100)::int) as message_text,
false as is_new,
timestamp '2023-06-10 20:00:00' +
       random() * (timestamp '2023-01-20 20:00:00' -
                   timestamp '2023-05-10 10:00:00') as timestamp_val
from generate_series(1,500)
;
