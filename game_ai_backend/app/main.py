from fastapi import FastAPI
from pydantic import BaseModel
import ollama
import base64
from io import BytesIO
from quickdraw import QuickDrawData # Nova biblioteca para rabiscos instantâneos
from PIL import Image

app = FastAPI(title="Pictionary AI Oponente Rápido")

print("A carregar o LLaVA para análise...")
# Ollama/LLaVA continua a ser necessário para a IA adivinhar o TEU desenho.
# Mas não precisamos de carregar o Stable Diffusion aqui.
print("LLaVA pronto!")

# Inicializamos o acesso aos dados do Quick Draw uma vez ao iniciar
qd_data = QuickDrawData()

class ImageRequest(BaseModel):
    ImageBase64: str
    Topic: str

class AiTextureRequest(BaseModel):
    Category: str # O tema geral
    Word: str     # A palavra exata a desenhar

@app.get("/hello-test")
def hello_test():
    return {"response": "Hello! O oponente IA está pronto para desenhar rápido!"}

@app.post("/analyze-drawing")
def analyze_drawing(request: ImageRequest):
    # --- MANTEMOS O TEU CÓDIGO ORIGINAL DO LLAVA AQUI ---
    prompt = (
        f"Atua como um jogador num jogo de adivinhas tipo Pictionary. "
        f"A imagem é apenas um simples rabisco ou esboço feito com traços pretos num fundo branco. "
        f"O tema do desenho é '{request.Topic}'. "
        f"Regras: "
        f"1. Não menciones cores, tipos de linha, fundos ou o facto de ser um desenho. "
        f"2. Foca-te puramente em adivinhar qual é a coisa dentro do tema '{request.Topic}' que aquelas linhas formam. "
        f"3. Responde de forma muito curta e direta, em inglês. "
        f"Diz-me apenas o que achas que o jogador tentou desenhar."
    )

    try:
        print("A processar a imagem com o LLaVA. Aguarda...")
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
        return {"Description": descricao}
        
    except Exception as e:
        return {"Description": f"Ocorreu um erro a analisar o desenho: {str(e)}"}
    

@app.post("/generate-drawing")
def generate_drawing(request: AiTextureRequest):
    # A palavra precisa de estar em inglês e normalizada (lowercase) para o QuickDraw
    word_to_draw = request.Word.lower().strip()
    print(f"IA Oponente está a 'desenhar' um(a) '{word_to_draw}' instantaneamente...")
    
    try:
        doodle = qd_data.get_drawing(word_to_draw)
        
        pil_image = doodle.image.resize((384, 384), Image.Resampling.LANCZOS)
        
        # Converter para Base64
        buffered = BytesIO()
        pil_image.save(buffered, format="PNG")
        img_str = base64.b64encode(buffered.getvalue()).decode("utf-8")
        
        print(f"Desenho de '{word_to_draw}' recuperado e enviado!")
        return {"ImageBase64": img_str}
        
    except ValueError:
        print(f"Erro: A palavra '{word_to_draw}' não existe no dataset QuickDraw.")
        # Se a palavra não existir, podes retornar uma imagem em branco ou 
        # um erro que o Unity trate, pedindo ao jogador para escolher outra palavra.
        return {"ImageBase64": "ERROR: Word not found in doodle dataset"}
    except Exception as e:
        print(f"Erro a recuperar a imagem: {e}")
        return {"ImageBase64": ""}