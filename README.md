# 3CX Spam Call Blocker

The program interacts with the 3CX API and the Yandex.ru “Who called” service. Spam, if so, then blocks it.
When launched, the “BlacklistCategories.txt” file is created in the 3CX directory, which can be independently supplemented.
A log is being kept.

Программа взаимодействует с 3CX API и сервисом Яндекс «Кто звонил». Вырубает спам если номер попадает в черный список сервиса «Кто звонил», взаимодействует с 3CX через API, к сожалению не нашел у 3CX события или соответствующего делегата о входящем звонке, поэтому в фоновом цикле прослушиваем и ожидаем звонок. При запуске в каталоге 3CX создается файл «BlacklistCategories.txt», который можно самостоятельно дополнять. Журнал ведется через NLog.

Uses API 3CX v3.0.0.0 (v16)