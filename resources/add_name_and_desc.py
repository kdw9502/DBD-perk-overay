#-*- coding: utf-8 -*-

import os
import json


with open("perk_info.json","r",encoding='UTF8') as file:

    file_string = file.read()
    dictionary = json.loads(file_string)
    
    perks = dictionary["perks"]
    for perk in perks:
        perk["name"] = input(f"{perk['fileName']} name : ")
        #perk["simpleDesc"] = input(f"{perk['fileName']} simple desc : ")



with open("new_perk_info.txt","w",encoding='UTF8') as new_file:
    new_file.write(json.dumps(perks,sort_keys=True, indent= 4, ensure_ascii=False))

