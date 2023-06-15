# OtusSocialNetwork. Инструкция по запуску

Запрос для создания таблиц лежит в папке `Postgres`.
Коллекция вызовов для Postman лежит в папке `Postman`.

1. Запускаем базу данных и API командой `docker-compose up -d`.
2. Ждем когда Postgres запустится, перезагрузится, создадутся таблицы.
3. Открываем Postman, создаем нового пользователя `qwerty` с паролем `qwerty`. Запрос `Register`. Получаем id пользователя.
4. Логинимся с полученным id. Запрос `Login`. Получаем токен авторизации
5. Получаем информацию о пользователе. Запрос `Get user by id`.

--- 

docker-compose up otus-db -d

cp ./Postgres/20230501-otusdb.sql ./volumes/Database/backups

docker exec -t otus-db psql -U dbuser -d otusdb -f /backups/20230501-otusdb.sql
