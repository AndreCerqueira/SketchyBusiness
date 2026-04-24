from fastapi import FastAPI
from pydantic import BaseModel
import ollama
import base64
from io import BytesIO
from quickdraw import QuickDrawData
from PIL import Image

app = FastAPI(title="Pictionary AI Oponente Rápido")

print("A carregar o LLaVA para análise...")
print("Servidor pronto!")

qd_data = QuickDrawData()

class ImageRequest(BaseModel):
    ImageBase64: str
    Topic: str

class AiTextureRequest(BaseModel):
    Category: str 
    Word: str     

class JudgeRequest(BaseModel):
    PlayerImageBase64: str
    AiImageBase64: str
    Topic: str
    Word: str

@app.get("/hello-test")
def hello_test():
    return {"response": "Hello! O oponente IA está pronto para jogar e julgar!"}

@app.post("/analyze-drawing")
def analyze_drawing(request: ImageRequest):
    prompt = (
        f"Act as a sassy and sarcastic player in a Pictionary game. "
        f"The image is a simple doodle drawn by a player trying to draw something related to the topic '{request.Topic}'. "
        f"Rules: "
        f"1. Start by making ONE very short, funny 'roast' (a sarcastic comment) about how bad, chaotic, or weird the player's drawing skills are. "
        f"2. After the roast, guess what the player was actually trying to draw within the topic '{request.Topic}'. "
        f"3. Keep your response short (max 10 words), direct, and write it in English."
        f"4. You MUST end your response by putting your final, single-word guess inside square brackets, exactly like this: [word]."
    )

    try:
        print("A processar a imagem do jogador com o LLaVA...")
        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    'content': prompt,
                    'images': [request.ImageBase64]
                }
            ]
        )
        descricao = resposta['message']['content']
        print(f"Resposta do LLaVA: {descricao}")

        return {
            "Description": descricao
        }
        
    except Exception as e:
        erro = f"Ocorreu um erro a analisar o desenho: {str(e)}"
        print(erro)
        return {
            "Description": erro
        }
    
@app.post("/judge-round")
def judge_round(request: JudgeRequest):
    prompt = (
        f"Act as a sassy and sarcastic Pictionary judge. "
        f"The topic is '{request.Topic}' and the target word both players had to draw is '{request.Word}'. "
        f"You will see two images: Image 1 is the Player's drawing. Image 2 is the AI's drawing. "
        f"Rules: "
        f"1. Roast the Player's drawing (Image 1) and then the AI's drawing (Image 2) in one short brutal sentence (max 10 words each). "
        f"3. Decide who drew the '{request.Word}' better. "
        f"4. You MUST end your response EXACTLY with the winner inside square brackets: [Player] or [AI]."
    )

    try:
        print("O LLaVA está a avaliar ambas as imagens para decidir o vencedor...")
        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    'content': prompt,
                    'images': [request.PlayerImageBase64, request.AiImageBase64]
                }
            ]
        )
        decisao = resposta['message']['content']
        print(f"Decisão do Juiz: {decisao}")
        return {"Result": decisao}
        
    except Exception as e:
        return {"Result": f"Erro ao julgar a ronda: {str(e)}"}

@app.post("/generate-drawing")
def generate_drawing(request: AiTextureRequest):
    word_to_draw = request.Word.lower().strip()
    print(f"A IA está a gerar um desenho de '{word_to_draw}'...")
    
    try:
        doodle = qd_data.get_drawing(word_to_draw)
        
        pil_image = doodle.image.resize((384, 384), Image.Resampling.LANCZOS)
        
        buffered = BytesIO()
        pil_image.save(buffered, format="PNG")
        img_str = base64.b64encode(buffered.getvalue()).decode("utf-8")
        
        print(f"Desenho de '{word_to_draw}' enviado!")
        return {"ImageBase64": img_str}
        
    except ValueError:
        print(f"Erro: A palavra '{word_to_draw}' não existe no dataset QuickDraw.")
        return {"ImageBase64": "ERROR: Word not found in doodle dataset"}
    except Exception as e:
        print(f"Erro a recuperar a imagem: {e}")
        return {"ImageBase64": ""}