#-*- coding: utf-8 -*-

import os
import json

perks = {"perks": []}
with open("perk_info.txt","r",encoding='UTF8') as file:

    file_string = file.read()


    dictionary = json.loads(file_string)

    for key, value in dictionary.items():
        new_dict = dict()
        new_dict["fileName"] = key
        new_dict["desc"] = value
        perks["perks"].append(new_dict)

with open("new_perk_info.txt","w",encoding='UTF8') as new_file:
    new_file.write(json.dumps(perks,sort_keys=True, indent= 4, ensure_ascii=False))

