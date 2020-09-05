#-*- coding: utf-8 -*-

import os
import json

perk_info = {}
with open("perk_info.txt","w") as file:
    for file_name in os.listdir("./Perks"):
        perk_name = file_name.replace(".png","")

        perk_desc = ""
        buffer = " "

        while buffer:
            buffer = input(perk_name + ": ")
            perk_desc += buffer
            perk_desc += "\n"

        perk_desc = perk_desc[:-1]

        perk_info[perk_name] = perk_desc
        print()

    file.write(json.dumps(perk_info, sort_keys=True, indent= 4, ensure_ascii=False))