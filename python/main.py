from PIL import Image, ImageDraw
import sys


# open image
def open_image(path):
    new_image = Image.open(path)
    return new_image


# save image
def save_image(image, path):
    image.save(path, "JPEG")


if __name__ == '__main__':
    # im = open_image("bec-map.jpeg")

    # box = (135, 188, 339, 306)
    # region = im.crop(box)

    with Image.open("bec-map.jpeg") as im:
        draw = ImageDraw.Draw(im, 'RGBA')
        # draw.line((135, 188) + (339, 188), fill=255, width=5)
        draw.rectangle([(135, 188), (339, 306)], (255, 0, 0, 85))
        # draw.line((0, im.size[1], im.size[0], 0), fill=128)
        del draw

    save_image(im, "edited.jpeg")

    # im.show()
    # im.save(sys.stdout, "PNG")
