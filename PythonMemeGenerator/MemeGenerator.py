from PIL import Image
import os
from os import listdir
import pyperclip
import win32clipboard
from io import BytesIO
import keyboard
import pyautogui
import psutil
import win32process
import win32gui
from PIL import ImageDraw
from PIL import ImageFont

def send_to_clipboard(clip_type, data):
    win32clipboard.OpenClipboard()
    win32clipboard.EmptyClipboard()
    win32clipboard.SetClipboardData(clip_type, data)
    win32clipboard.CloseClipboard()

BasePath = 'E:\Memes uteis\\'
Dict = {}
def RepopulateDictionary():
    Dict.clear()
    folder_dir = BasePath
    Dict['repopulate'] = 'repopulate'
    for image in os.listdir(folder_dir):
        if image.endswith('.png') or image.endswith('.jpg'):
            Dict[image[0:-4].lower()] = BasePath+image

AllowedApps = {}
def GetAllowedApps():
    file = open(BasePath+'AllowedApps.txt')
    lines = file.read().splitlines()
    for constraint in lines:
        AllowedApps[constraint.lower()] = ''

def MakePieces(text, x, xEnd, font):
    text_size = font.getlength(text)
    fitSize = xEnd - x 
    if text_size < fitSize:
        return text
    
    possibleSize = 'ABCDEFGHIJKLMNOPQRSTUVXWYZabcdefghijklmnopqrstuvxwyz'
    fontAverage = font.getbbox(possibleSize)[2] / len(possibleSize)
    fitCount = int(fitSize / fontAverage)
    resultText = insert_newlines(text, fitCount)

    return resultText

def insert_newlines(string, every):
    lines = []
    for i in range(0, len(string), every):
        if (len(string) >= i+every and string[i+every] != ' '):
            lines.append(string[i:i+every] + '--')
        else:
            lines.append(string[i:i+every])

    return '\n'.join(lines)

GetAllowedApps()
RepopulateDictionary()
while True:
    try:
        pyperclip.waitForNewPaste()
        copiedText = pyperclip.paste()
        additionalText = ""
        if ";" in copiedText:
            splited = copiedText.split(";")
            copiedText = splited[0]
            additionalText = splited[1]

        currentImg = Dict.get(copiedText.lower())
        hwnd = win32gui.GetForegroundWindow()
        _,pid = win32process.GetWindowThreadProcessId(hwnd)
        process = psutil.Process(pid)
        currentProcess = process.name().lower()
        if currentImg != None and currentProcess in AllowedApps:#Intentional constraint
            if currentImg == 'repopulate':
                RepopulateDictionary()
            else: 
                image = Image.open(currentImg).convert("RGBA")
                output = BytesIO()
                if len(additionalText) > 0:
                    file = open(BasePath+copiedText+'.txt')
                    lines = file.readlines()
                    
                    tempSplit = lines[0].split(',')
                    x = int(tempSplit[0])
                    y = int(tempSplit[1])
                    tempSplit = lines[1].split(',')
                    xEnd = int(tempSplit[0])
                    yEnd = int(tempSplit[1])
                    tempSplit = lines[2].split(',')
                    fontSize = int(tempSplit[0])
                    fontColor = tempSplit[1]

                    squareImg = Image.new('RGBA', (xEnd - x, yEnd - y), (0,0,0,0))
                    imgDraw = ImageDraw.Draw(squareImg)
                    imgFont = ImageFont.truetype("ariblk.ttf", fontSize)
                    additionalText = MakePieces(additionalText, x, xEnd, imgFont)
                    imgDraw.text((0,0), additionalText, fontColor, font=imgFont)
                    image.paste(squareImg, (x, y), mask=squareImg)

                image.save(output, "BMP")
                data = output.getvalue()[14:]
                output.close()
                send_to_clipboard(win32clipboard.CF_DIB, data)
                pyautogui.keyDown('ctrl')
                pyautogui.keyDown('v')
                pyautogui.keyUp('v')
                pyautogui.keyUp('ctrl')
    except Exception as e:
        pass