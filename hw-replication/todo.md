Создаем сеть
docker network create pgnet
docker network inspect pgnet | grep Subnet

"Subnet": "172.18.0.0/16"

Запускаем мастер

docker run -dit \
-v $PWD/pgmaster-data/:/var/lib/postgresql/data \
-v $PWD/pgmaster-backups/:/backups \
-e POSTGRES_PASSWORD=docker -p 15432:5432 \
--restart=unless-stopped \
--network=pgnet \
--name=pgmaster \
postgres:14

Сделаем бекап
sudo docker exec -it pg-docker-14 pg_dump -U postgres -d otusdb -f /backups/20230501-otusdb.sql

Восстанавливаем дамп базы
sudo docker exec -it pgmaster psql -U postgres -d otusdb -f /backups/20230501-otusdb.sql

делаем бекап для реплики
docker exec -it pgmaster pg_basebackup -h pgmaster -D /backups -U replicator -v -P --wal-method=stream --checkpoint=fast

Копируем бекап на слейв
sudo cp -r ./pgmaster-backups/* ./pgslave-data
sudo cp -r ./pgmaster-backups/* ./pgslave-second-data

Создадим файл, чтобы реплика узнала, что она реплика
touch pgslave-data/standby.signal
touch pgslave-second-data/standby.signal

Меняем postgresql.conf на реплике
primary_conninfo = 'host=pgmaster port=5432 user=replicator password=reppassword application_name=pgslave'

Запускаем реплику
docker run -dit \
-v $PWD/pgslave-data/:/var/lib/postgresql/data \
-e POSTGRES_PASSWORD=docker -p 25432:5432 \
--restart=unless-stopped \
--network=pgnet \
--name=pgslave \
postgres:14
---




делаем бекап

docker exec -it replication-1-pgmaster-1 pg_basebackup -h pgmaster -D /backups -U replicator -v -P --wal-method=stream --checkpoint=fast





Запускаем реплику

docker-compose -f docker.replication-1.yml up pgslave

