import os

for file_name in os.listdir("./Perks"):
    new_name = file_name.replace("iconPerks_","")
    new_name = new_name[0].lower() + new_name[1:]
    os.rename("./Perks/" + file_name, "./Perks/"+ new_name)