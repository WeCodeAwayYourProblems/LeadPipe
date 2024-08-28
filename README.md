# How to transform this template into your own
=======
## Which dependencies already exist in this solution?
Look at the csproj files, you goose.
=======
## Reset the git
After you've cloned this repo to your local machine, you'll want to reset the git so that you aren't changing the template
To do this, simply delete the entire git folder. **Do not** delete the .gitignore file, just the hidden .git folder. You can do this by opening the solution in VS Code, which allows you to see all hidden folders.

### Resetting the .gitignore
If you've already deleted the .gitignore file, don't worry. I've provided the templatized .gitignore contents here:
```
#b
**/bin

#i
# .info is my personal favorite custom folder and file name that I use to hide files from git
# All you have to do is name a folder or file with .info at the beginning and that folder or file will automatically be ignored
# For example, .info=ThisFileContainsSecrets.ext will automatically be ignored, and .info=ThisFolderContainsSecrets will also be ignored
.info/ 
/.info
**/.info
**/.info**
**/.info**/

#o
**/obj

#v
/.vs

# Added Later

```

### Establishing your own git
I found that the best way to do this was to just create another repo using the extremely simple and easy to use github for desktop. It is so much easier to establish a remote using that. So do it that way.
=======
## Naming conventions for git commits
### DEFINITION
Version naming convention follows this simple numerical system:
[Major Update Number].[Minor Update Number].[Patch Number--Debugging].[Build Number]

- "Major update" involves possible backward compatibility conflicts or major feature updates that require a significant rebuild 
	- Like adding functional programming to the solution, for example
- "Minor update" involves feature adds that are unlikely to have serious functional conflicts
	- Like adding a feature that converts a json file to a csv file, for example
- "Patch number" is the number of bug fixes or security pathes on the current build, shuddup, whaddya want?
- "Build number" is a reference to the software build version number and is often left out

### EXAMPLE
v2.4.2 
This means that we're on the second major release, that we're on the fourth minor feature add, and two bug fixes have been performed on the most recent update

### USE
3rd - When a bug fix goes through, the third number is incremented. 
2nd - When a minor update goes through, the second number is incremented and the patch number (the third number), is set to zero
1st - When a major update goes through, the minor update number is set to zero AND the patch number is set to zero

4th - Who knows when the fourth number is incremented, why are you asking me?
=======
## Rename the Solution
**Do not complete this step while the solution is loaded in Visual Studio. ONLY load the solution folder in VS Code.**
The solution is still named Template instead of "YourSolution". To fix this, rename the .sln file **and** the folder in which it's located. For example, right now, the solution *folder* is named "Template" instead of "YourSolution". Inside the actual .sln file itself, you'll have to change the relative path name of the .sln file 

### Rename the csproj files
**Do not complete this step while the solution is loaded in Visual Studio. ONLY load the solution folder in VS Code.**
You'll notice that the solution name is 'Template'. In your own solution, you'll want to rename it to something that actually makes sense for your situation.

1. Open the solution folder in VS Code
2. Rename each of the projects to your chosen solution name. e.g. You'll want to rename "Application.csproj" to "YourSolution.Application.csproj" and so on for each of the projects
3. You'll now want to rename the folders that each of the project files are located in. e.g.You'll want to rename the folder entitled "Application" to "YourSolution.Application" and so on for each project
4. Once you've done step 3, your entire solution will break. To fix this, rename the paths located in the .sln file **and** each of the .csproj files to match the relative paths of each of the .csproj files.

### Adjust the namespaces
Now that you've adjusted these things with such painstaking effort, now it's time to test you will.
Rename all the namespaces.

You see, the file structure is different now. Because of that, the namespaces are also technically different.
You don't actually have to do this, but it'll bother you until you do it. So do it, you goose.

It's easiest just to start at the top of the file explorer in Visual Studio and work your way down. It might be possible to Ctrl + . your way out of it, but it might not work, so it's best to double check

### Rebuild the solution
Now that you've done all of this in VS Code, it's finally time to open your solution in Visual Studio. Hurray!
Just make sure that it builds. If it doesn't build, you'll have to fix some problems.
But you've already adjusted the git, so you can't change it in the repo, hardy har.