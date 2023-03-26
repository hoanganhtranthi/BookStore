drop database BookStore
Go
create database BookStore
GO
use BookStore
CREATE TABLE Users(
	UserId int identity(1,1) primary key not null,
	DateOfBirth datetime not null,
	Address nvarchar(100),
	Gender varchar(10),
	UserName nvarchar(30) not null,
	PasswordHash varbinary(max) not null,
	PasswordSalt varbinary(max) not null,
	VerificationToken nvarchar(max),
	VerifiedAt datetime null,
	PasswordResetToken nvarchar(max) null,
	ResetTokenExpires datetime null,
	Email varchar(50)not null,
	role varchar(10)
	)
GO
CREATE TABLE OrderBooks(
	OrderId int identity(1,1) primary key not null,
	OrderDate datetime null ,
	OrderReturnDate datetime null,
	Status int not null,
	TotalPrice float not null,
	UserId int foreign key references Users(UserId )
)
GO
CREATE TABLE Categories(
	CateId int identity(1,1) primary key not null,
	CateName nvarchar(50)
)
GO
CREATE TABLE Books(
	BookId int identity(1,1) primary key not null,
	Price float not null,
	BookImg nvarchar(50) not null,
	BookName nvarchar(50) not null,
	CurrentQuantity int not null,
	BookDetail nvarchar(1000),
	CateId int foreign key references Categories(CateId)
)
GO

CREATE TABLE OrderDetails(
	OrderDetailId int identity(1,1) primary key not null,
	OrderId int foreign key references OrderBooks(OrderId),
	BookId int foreign key references Books(BookId),
	ReturnedQuantity int ,
	Quantity int not null,
	Price float,
	BookName nvarchar(50) not null
)
GO
use BookStore
Insert into Users values ('2002-10-18','Ho Chi Minh City','Male','admin',Convert(varbinary(max),'admin'),Convert(varbinary(max),'admin'),'1999',null,null,null,'admin@gmail.com','Admin')
Insert into Users values ('2000-09-18 ','Ho Chi Minh City','Female','vinhhung123',Convert(varbinary(max),'123455'),Convert(varbinary(max),'123456'),'2445',null,null,null,'vinhhung@gmail.com','Customer')
Insert into Users values ('2001/01/01','Ha Noi','Male','hieuvuong123',Convert(varbinary(max),'123455'),Convert(varbinary(max),'123456'),'2445',null,null,null,'hieuvuong@gmail.com','Customer')
Insert into Users values ('2002/10/16','Thua Thien Hue','Female','hanh123',Convert(varbinary(max),'123455'),Convert(varbinary(max),'123456'),'2445',null,null,null,'hanh@gmail.com','Customer')
Insert into Users values ('2002/04/22','Da Nang','Male','mytam123',Convert(varbinary(max),'123455'),Convert(varbinary(max),'123456'),'2445',null,null,null,'mytam@gmail.com','Customer')
Insert into Users values ('2002/05/30','Tay Ninh','Female','sontung123',Convert(varbinary(max),'123455'),Convert(varbinary(max),'123456'),'2445',null,null,null,'sontung@gmail.com','Customer')

insert into Categories(CateName) values('Phe binh van hoc'),('Tieu thuyet'),('Truyen & Tho Ca Dan Gian'),('Truyen Gia Tuong – Than Bi'),('Phong Su, Ky Sy'),('Tho Ca ')

Insert into Books values(30.000,'book1.png','Ra Bo Suoi Ngam Hoa Ken Hong',30,'Ra Bo Suoi Ngam Hoa Ken Hong la tac pham trong treo, tran đay tinh yeu thương mat lanh, trai ra truoc mat nguoi đoc khu vuon trai ruc ro co hoa cua vung que thanh binh',2)
Insert into Books values(40.000,'book2.png','Chua Kip Lon Đa Truong Thanh',25,'Chung ta của hien tai, deu chua kip lon đa phai truong thanh.',4)
Insert into Books values(45.000,'book3.png','Mui Hech',15,'Cau chuyen khong ke ve nhân vat chinh voi cuoc đoi đay bao to, nhung co nang Mui Hech (biet danh đang yeu cua nhan vat Le Ha Anh trong truyen) đa song mot doi that tron ven.',5)
Insert into Books values(75.000,'book4.png','Qua dua hau khong lo',20,'Tac pham cua Naoko Shono duoc xuat ban lan dau nam 2014 voi nhan vat chinh la hai chu meo Rukkio và Furifuri',3)
Insert into Books values(80.000,'book5.png','Ethon-Mot Sach Mogu-Xin Moi Ngoi',10,'Hinh anh cac ban dong vat vo cung dang yeu, moi ban deu co mot chiec ghe ngoi yeu thich cua minh. Cac ban nho hay cung quan sat chiec ghe ngoi cua moi ban co diem gu thu vi nhe!',6)
