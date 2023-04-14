# ShadowKernel - Дипломная работа "ПО для аудита информационной безопасности"

<a href="https://github.com/Supermegatiger/ShadowKernel/releases/download/1.1/Debug.rar"><img src="https://github.com/Supermegatiger/ShadowKernel/blob/master/ShadowKernel/assets/icon.ico" width="24"> - download</a>

## Реализация клиент-серверного взаимодействия (получение информации с компьютеров в локальной сети и мониторинг):
* получение информации о компьютере;
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134538565-52754f52-6fef-4da3-a041-768d4c6677be.png"></p>

* получение списка установленных на компьютере приложений;
* получение списка запущенных процессов на компьютере и возможность удалять процессы;
* получение списка «активных» портов на компьютере клиента и возможность удалять процессы, использующие «активный» порт;
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134540008-888f4c32-f532-4ff6-adc6-39b6317ca724.png"></p>

* получение загруженности ЦП, ОЗУ и системного диска в процентах;
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134540177-155d3e81-0f7e-4069-a0f0-324cffa5101f.png"></p>

* получение файлов на компьютере, возможность загружать файлы на компьютер, загружать файлы с компьютера, удалять файлы;
* удалённая командная строка/PowerShell;
* просмотр экрана;
* «кейлоггер» и получение названия активного окна;
* возможность перевести компьютер в спящий режим, выключить, перезагрузить, выйти из системы, заблокировать пользователя и заблокировать экран;
* текстовый чат;
* анализатор сети (сниффер).
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134542381-9e78ba54-92fe-4c90-a4b3-6af2bf3e1e31.png"></p>


## Реализация проведения аудита информационной безопасности:
* создание базы данных с помощью СУБД SQLite с моделями: аудитор, аудит, вопрос, категория, ответ;
* формирование списка вопросов и разбиение по категориям;
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134537150-e5d6f391-7672-4a17-ad11-22e3932c1b20.png"></p>

* формирование отчёта по проведённому аудиту в виде таблицы с категориями вопросов и процентах соответствия норме по каждой из них. Возможность получения отчёта в виде pdf-файла;
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134537631-504c3d31-deb6-47e1-9c1f-9307c46fcf26.png"></p>

* правильное логическое разделение проведения аудитов;
<p align="center"><img src="https://user-images.githubusercontent.com/68782056/134537888-f06c13cb-70fd-446b-b189-60b29aec3a93.png"></p>

* реализация ограничения доступа к информации путём создания окна входа и окна регистрации нового аудитора.
<table><tr><td>
<img align="left" src="https://user-images.githubusercontent.com/68782056/134526841-e904ad8d-8028-452e-b9f5-b9fd82889f21.png"></td><td>
<img align="right" width="570" src="https://user-images.githubusercontent.com/68782056/134538048-6ae6a31f-7ca3-4dc7-a535-feb53f42e8b2.png"></td></tr></table>
</br></br>

