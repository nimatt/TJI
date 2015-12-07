# TJI
Toggl Jira Integration is made to simplify time tracking in Jira. Toggl enables users to log time in a convenient way but teams using Jira will most likely want to be able to follow progress and time spent in that system. So this program was written to remove the need for duplicated logging. It uses the description of you Toggl entry to find the correct issue and add or synchronize the corresponding item in its worklog.

The program is still in an early stage and should not be considered stable.

## How to use
### Installation
Download run msi-package from release page.

### Settings
The main window contain a _Settings_ button that opens a window with input for
* Jira settings
  * Url to Jira server
  * Jira username
  * Jira password
* Toggl settings
  * API Token
* Advanced settings
  * Sync interval in seconds

### Usage
TJI will automatically start to synchronize if valid settings are found on startup. Toggl is then queried for new entries after the sync interval given in settings (default is every 10 min). Synchronization can be manually started/stopped by using the button _Start_/_Stop_ in the main window.
