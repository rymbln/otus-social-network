-- public.account definition

CREATE TABLE IF NOT EXISTS public.account (
	id varchar NOT NULL,
	"password" varchar NOT NULL,
	CONSTRAINT account_pk PRIMARY KEY (id)
);

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


-- public.friends definition

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