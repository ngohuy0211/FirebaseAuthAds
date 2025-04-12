1. Package này chỉ là add các SDK và code kết nối
2. Người dung cần tạo app trên Firebase, Meta Developer
3. Nếu dung ads thì cần tạo app trên Google Admob và Google AdSense
4. Cần config rất nhiều key trong cả unity và cả các app - xem qua video hướng dẫn: https://www.youtube.com/@ZGamesStudio
5. Đối với ADS thì cần tạo các unit trên admob và config id unit vào scriptTableObject trong unity
(Create/Ads/...)
6. Apple Login cần tạo app trên Apple Developer
7. Facebook Login cần tạo các link policy, link hướng dẫn user xóa ac, phải có 2 link này thì lúc up lên store mới chuyển được từ devmode sang livemode trên meta developer
8. Sau khi forceResolve Android. Cần kiểm tra lại file settingsTemplate trong Plugins/Android và sửa lại đường dẫn
của GoogleSign thành GeneratedLocalRepo/GoogleSignIn
