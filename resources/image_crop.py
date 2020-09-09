import os
from PIL import Image

for root, dirs, files in os.walk('./Perks'):
    for file in files:
            image = Image.open('./Perks'+"/"+file)
            crop_image = image.crop((64, 64, 128+64, 128+64))
            crop_image.save('perks_crop/'+file)       