-- public.account definition

CREATE TABLE public.account (
	id varchar NOT NULL,
	"password" varchar NOT NULL,
	CONSTRAINT account_pk PRIMARY KEY (id)
);

-- public."user" definition

CREATE TABLE public."user" (
	id varchar NOT NULL,
	first_name varchar NOT NULL,
	second_name varchar NOT NULL,
	sex varchar NOT NULL,
	age int4 NOT NULL,
	city varchar NOT NULL,
	biography varchar NOT NULL,
	CONSTRAINT user_pk PRIMARY KEY (id)
);