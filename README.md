# PostgreSQL. Table Partitioning

- [5.11. Секционирование таблиц](https://postgrespro.ru/docs/postgresql/16/ddl-partitioning)
- [npgsql / efcore.pg. Support for table partitioning #1035 (Open from 2019)](https://github.com/npgsql/efcore.pg/issues/1035)

### Поднятие и подключение к БД PostgreSQL

1. Установить [докер](https://www.docker.com/)
2. В корне проекта лежит файл `docker-compose.yaml`
2. В терминале надо перейти в папку проекта и выполнить команду:

```shell
docker-compose up
```

3. Строка подключения к БД:

```
"host=127.0.0.1;port=5443;database=db;username=root;password=12345"
```