from PIL import Image, ImageDraw
import sys
import os
from pixels import bec_dict, pft_dict


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
def highlight_image(file, building_dict, room_number):
    # grab correct dictionary
    building_dict = find_dict(building_dict)

    room_number = str(room_number)

    # open image file
    with Image.open(file) as im:
        draw = ImageDraw.Draw(im, 'RGBA')
        draw.rectangle([(building_dict[room_number][0], building_dict[room_number][1]),
                        (building_dict[room_number][2], building_dict[room_number][3])], (255, 0, 0, 85))
        del draw

    save_image(im, "edited.jpeg")
    path = os.path.abspath("edited.jpeg")
    print(f"\t* Saved new file at {path} *")


def main(file, building_dict, room_number):
    print("\t**********************************************")
    print("\t**** Greeter - STARTED PYTHON FILE CALL. *****")
    print("\t**********************************************")

    highlight_image(file, building_dict, room_number)


if __name__ == '__main__':

    # CALLING THIS FILE INSTRUCTIONS:
    # python main.py main file building_dict room_number
    # python main.py main bec-map.jpeg bec 1620

    # main(), main, building_dict, room_number
    globals()[sys.argv[1]](sys.argv[2], sys.argv[3], sys.argv[4])

    # main("bec-map.jpeg", "bec", "1620")
