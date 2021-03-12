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
    # grab correct dictionary
    building_dict = find_dict(building_dict)

    room_number = str(room_number)

    # open image file
    with Image.open(file) as im:
        font_size = 100
        draw = ImageDraw.Draw(im, 'RGBA')
        draw.rectangle([(building_dict[room_number][0], building_dict[room_number][1]),
                        (building_dict[room_number][2], building_dict[room_number][3])], (255, 0, 0, 95))
        font = ImageFont.truetype(font_path, font_size)
        # draw text, xy pixels, text, fill color, font
        draw.text((25, 74), "Room:" + room_number, fill='black', font=font)
        del draw

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