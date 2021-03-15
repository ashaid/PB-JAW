from PIL import Image, ImageDraw, ImageFont
import sys
import os
from pixels import bec_dict, pft_dict
import pathlib


# open image
def open_image(path):
    new_image = Image.open(path)
    return new_image


# save image
def save_image(image, path):
    image.save(path, "JPEG")


def find_dict(building_dict):
    if building_dict == "bec":
        building_dict = bec_dict
    elif building_dict == "pft":
        building_dict = pft_dict

    return building_dict;


# highlights correct area
def highlight_image(file, building_dict, room_number, name):
    font_path = str(pathlib.Path().absolute().parent) + f"\\pb-jaw\\wwwroot\\css\\Font\\TIMES.TTF"
    # grab correct dictionary and room number
    building_dict = find_dict(building_dict)
    room_number = str(room_number)

    # variables for watermark
    water_mark = "PB-JAW"
    font_size = 100
    margin = 5

    # open image file
    with Image.open(file) as im:
        font = ImageFont.truetype(font_path, font_size)
        # draw rectangle
        draw = ImageDraw.Draw(im, 'RGBA')
        draw.rectangle([(building_dict[room_number][0], building_dict[room_number][1]),
                        (building_dict[room_number][2], building_dict[room_number][3])], (255, 0, 0, 95))

        # draw text, xy pixels, text, fill color, font (drawing room number on image)
        draw.text((25, 74), "Room:" + room_number, fill='black', font=font)

        # calculate x,y coordinates of text
        width, height = im.size
        # print(width + " " + height)
        text_width, text_height = draw.textsize(water_mark, font)
        x = width - text_width - margin
        y = height - text_height - margin
        # draw watermark
        draw.text((x, y), water_mark, font=font, fill='black')

        del draw

    # grabs path to created image
    cd = str(pathlib.Path().absolute().parent) + f"\\pb-jaw\\wwwroot\\created\\{name}"

    try:
        save_image(im, cd)
    except Exception as err:
        print(format(err))

    print(f"\t* Saved new file at {cd} *")


def main(file, building_dict, room_number, name):
    print("\t**********************************************")
    print("\t**** Greeter - STARTED PYTHON FILE CALL. *****")
    print("\t**********************************************")

    highlight_image(file, building_dict, room_number, name)


if __name__ == '__main__':

    # CALLING THIS FILE INSTRUCTIONS:
    # python main.py main file building_dict room_number
    # python main.py main bec-map.jpeg bec 1620

    # main(), main, building_dict, room_number name_of_new_image
    globals()[sys.argv[1]](sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5])

    # main("bec-map.jpeg", "bec", "1620")