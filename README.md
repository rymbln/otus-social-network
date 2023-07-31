# OtusSocialNetwork. Инструкция по запуску

Для запуска используем `docker-compose up -d`.

После этого заходим в браузере на localhost:4200.
Нажимаем кнопку Login. Данные для входа уже предзаполнены.

В системе зарегистрировано три логина:
3eefe556-a733-4ba9-ba28-3e55cc459a79
b2428a06-ee2a-40b8-94f4-69cd3c85a2e0
3cc744e5-ec95-4926-9a64-aba219819337

У всех пароль qwerty

## Написание постов

Для написания постов перейти http://localhost:4200/posts и нажать кнопку New.

После отправки поста, у всех друзей появится всплывающее сообщение о новом посте.

Если у друга открыта страница новостей http://localhost:4200/feed то на ней появится новый пост.

## Запуск контейнеров

```
# Запуск citus
docker compose up -d citus-master citus-manager citus-worker-1 citus-worker-2 citus-worker-3

docker compose up -d citus-worker-4 citus-worker-5

docker compose up -d pgbouncer citus-master citus-manager citus-worker-1 citus-worker-2 citus-worker-3 citus-worker-4 citus-worker-5

# Запуск rabbit

docker compose up -d otus-haproxy otus-rmq0 otus-rmq1 otus-rmq2

# Запуск tarantool

docker compose up -d otus-tarantool

# Запуск api
docker compose up -d otus-api-1 otus-api-2 otus-api-3 otus-web

# Запуск grpc

docker compose up -d otus-dialogs


```

### Задание Масштабируемая подсистема диалогов

1. Запускаем citus-postgres `docker compose up -d citus-master citus-manager citus-worker-1 citus-worker-2 citus-worker-3`
2. Запускаем запрос из файла `postgres\create-tables.sql` чтобы создать нужные таблицы
   1. Ключ шардирования выбран `user_id` для таблиц `posts`, `friends`
   2. Ключ шардирования выбран `from_user_id` для таблицы `messages` потому что это позволит получать сообщения диалогов максимум из двух "шардов". А если повезет, то и из одного

3. Запускаем rabbit `docker compose up -d otus-haproxy otus-rmq0 otus-rmq1 otus-rmq2`
4. Запускаем tarantool `docker compose up -d otus-tarantool`
5. Запускаем api `docker compose up -d otus-api-1 otus-api-2 otus-api-3 otus-web`
6. Проверяем работу диалогов с помощью postman методов api/Dialogs
7. Запускаем дополнительные шарды `docker compose up -d citus-worker-4 citus-worker-5`
8. Подключаемся к citus-master и запускаем `SELECT rebalance_table_shards();` Начинается решардинг.
9. Параллельно можно запускать вызовы в Postman - они выполняются.
10. Дожидаемся завершения процесса решардинга.

### Задание Масштабируемая подсистема диалогов

1. Запускаем citus-postgres `docker compose up -d pgbouncer citus-master citus-manager citus-worker-1 citus-worker-2 citus-worker-3 citus-worker-4 citus-worker-5`
2. Запускаем запрос из файла `postgres\create-tables.sql` чтобы создать нужные таблицы
3. Запускаем rabbit `docker compose up -d otus-haproxy otus-rmq0 otus-rmq1 otus-rmq2`
4. Запускаем tarantool `docker compose up -d otus-tarantool`
5. Запускаем api `docker compose up -d otus-api-1 otus-api-2 otus-api-3 otus-web`
6. Запускаем gRPC сервис с диалогами `docker compose up -d otus-dialogs`
7. Проверяем работу диалогов с помощью postman методов для REST и для GRPC
8. Для совместимости, вызовы по REST для api/dialogs оставлены, внутри они вызывают gRPC-сервис с помощью клиента DialogsService
9. Описание сервиса в webapi\OtusClasses\Proto\dialogs.proto

### Задание Распределенные транзакции

1. Запускаем citus-postgres `docker compose up -d pgbouncer citus-master citus-manager citus-worker-1 citus-worker-2 citus-worker-3 citus-worker-4 citus-worker-5`
2. Запускаем запрос из файла `postgres\create-tables.sql` чтобы создать нужные таблицы
3. Запускаем rabbit `docker compose up -d otus-haproxy otus-rmq0 otus-rmq1 otus-rmq2`
4. Запускаем tarantool `docker compose up -d otus-tarantool`
5. Запускаем api `docker compose up -d otus-api-1 otus-api-2 otus-api-3 otus-dialogs otus-counters`
6. В постман отправляем сообщение /api/Dialog/:userId/send

https://github.com/rymbln/otus-social-network/tree/026-hw

Сервис счетчиков otus-counters - обновляет счетчики в Tarantool после отправки сообщения
Сервис диалогов otus-dialogs - сохраняет и удаляет сообщения
Сервис приема запросов и отправки сообщений по webscoket - otus-web-api

Описание SAGA https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusClasses/Sagas/SendMessageSaga.cs
Используемые сообщения для SAGA https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusClasses/Sagas/Events/SendMessageSagaEvents.cs

Порядок действий следующий:
1. Пользователь отправляет запрос на создание нового сообщения, создается сообщение MessageCreatedEvent и запускается сага https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusWebApi/Controllers/DialogController.cs
2. Сохраняем сообщение (SaveMessageEvent) с помощью otus-dialogs https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusDialogsGrpc/Handlers/SaveMessageEventHandler.cs
3. Если сообщение не удалось сохранить, то отправляем пользователю PushMessageFailedEvent и завершаем сагу с помощью MessageFailEvent https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusWebApi/Handlers/PushMessageFailedEventHandler.cs
4. Если сообщение удалось сохранить, то отправляем пуш адресату PushMessageEvent по вебсокету https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusWebApi/Handlers/PushMessageEventHandler.cs
5. Запускаем обновление счетчиков UpdateCountersEvent https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusChatCounters/Handlers/UpdateCountsEventHandler.cs
6. Если обновление счетчиков не удалось, запускаем компенсирующую транзакцию DeleteMessageEvent https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusDialogsGrpc/Handlers/DeleteMessageEventHandler.cs
7. Если обновление счетчиков удалось, отправляем новые счетчики адресату PushCountersEvent https://github.com/rymbln/otus-social-network/blob/026-hw/webapi/OtusWebApi/Handlers/PushCountersEventHandler.cs
8. Завершаем Сагу
