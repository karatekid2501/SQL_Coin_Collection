# SQL Coin Collection (v1.0)
This project allows coin collectors to easily keep track on which coins they have got in their collection.
The manual can either be accessed by clicking the Help button on the main window in the application or here:
- [Current Version](https://olivermwhite.wixsite.com/website/ccm-v1-0)
- [Version List](https://olivermwhite.wixsite.com/website/sql-coin-coll)

## Requirments
No special requirements

## Installation
- Click on Releases and then click on the Coin Collection <version number> zip folder.
- Once the zip is download it, right click on it and click on extract.
- Once its extracted, you can go to the folder and click on CoinCollection.exe to start the application.
- **Note:** It’s recommended to move the extracted folder from the Download folder, so it will not get accidentally deleted if the folder is cleared out.

## Configeration
If you want to edit the config file, then you’ll need to navigate to the application folder, then open appsettings.json. You can use any application to edit the config file, but I recommend using [Notepad++](https://notepad-plus-plus.org/downloads/)

## FQA
**Q: What if I accidentally delete the config file?**

**A:** If the application detects that the appconfigeration is missing, it will recreate it. This does mean that any settings saved will be reset and you’ll have to reattach your database.

**Q: Would creating or connecting another database delete the current one?**

**A:** No, if a database is already connected and you want to connect or create a new one, the current one will not be deleted.

**Q: Can I backup my database?**

**A:** Currently no but will be added in a future update.

**Q: I currently have a bug/issue/crash that is not been reported**

**A:** If you are experiencing an issue, then you can create a discussion thread on the GitHub page of the project under the issues tab.

**Q: I’ve deleted an image of a coin but it’s still showing up**

**A:** This is more likely that you have deleted the original image of the coin and not the copied one that is stored in the Images folder. Deleted that and it should be removed from the application. If you have done that while the application is still running, clicking on the Update button in the Modify/New Coin Window, should remove it from the list.

## Release Notes

### v1.0
- Create and select databases for the application
- Add and remove coins
- Clear the database
- View selected coins within the database
- Enable and disable the report system
- Change the frequency of the report system