ApparelPro is a fully supported application system in Apparel Industry sector. It covers everything. ApparelPro (here in after refers to AP) has many modules 
1 Reference files
2 Order Management (By merchandisers and Merchandising managers)
3 Order Wise Inventory
4 General Inventory (for general/bulk purchases)
5 PO creation.
6 Style creation 
7 Material consumption.
8 Color/Zize breakdown
9 Order confirmation
10 Production Control (Cut/saw/pack)
11 Line Allocation
12 Import/Export Documentation (including Packing list/CUSDEC (Customs Declarations)/Quota Register etc.
13 Quota Register
14 Shipment/Part Shipment
15 Order Approval
16 Sub Contract


Technologies used 

Backend

1 Language: C# 
2 Platform: .NET Core
3 Database: SQL Server 2019
4 Backend technologies 
Fully utilized and database creation using EF Core 8.0
Fluent Migration/Auto Mapper/Distributed caching using SQL Server

API 
100% use of RESTful API. 
Fully support Authentication using JWT token/refreshing
Fully support Authorization using registered user (logged in ) or several created roles (such as Merchandiser, Order Entry Operator, Merchandiser Manager, Inventory, Production, Import Export, OfficeAdmin

3 layered architecture front end, middle tier (Service tier) and backend with database 
Using front end APIModels, Service Models and database models.

Front End
Fully using Angular 17
Customize base table class and base form class 
Using DI with Injector for services and values 
RXjs operations such as Behaviour Subject, map, Observable, signals etc.

Angular Materials library.
CSS, Bootstrap and tailwindCSS





