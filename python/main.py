from PIL import Image, ImageDraw
import sys
import os


# open image
def open_image(path):
    new_image = Image.open(path)
    return new_image


# save image
def save_image(image, path):
    image.save(path, "JPEG")


# highlights correct area
def highlight_image(file):
    with Image.open(file) as im:
        draw = ImageDraw.Draw(im, 'RGBA')
        draw.rectangle([(135, 188), (339, 306)], (255, 0, 0, 85))
        del draw

    save_image(im, "edited.jpeg")
    path = os.path.abspath("edited.jpeg")
    print(f"\t* Saved new file at {path} *")


def main(file):
    print("\t**********************************************")
    print("\t**** Greeter - STARTED PYTHON FILE CALL. *****")
    print("\t**********************************************")

    highlight_image(file)


if __name__ == '__main__':
    globals()[sys.argv[1]](sys.argv[2])
