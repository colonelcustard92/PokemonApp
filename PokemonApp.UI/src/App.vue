<template>
  <div class="container py-5">
    <h1 class="text-center mb-4">Pokédex</h1>

    <div v-if="!token" class="card mx-auto" style="max-width: 400px;">
      <div class="card-body">
        <h5 class="card-title">Login</h5>
        <div class="mb-3">
          <label class="form-label">Username</label>
          <input v-model="username" type="text" class="form-control" />
        </div>
        <div class="mb-3">
          <label class="form-label">Password</label>
          <input v-model="password" type="password" class="form-control" />
        </div>
        <button @click="login" class="btn btn-primary w-100">Login</button>
        <div v-if="error" class="alert alert-danger mt-3">{{ error }}</div>
      </div>
    </div>

    <div v-else>
      <div class="input-group mb-4">
        <input v-model="pokemonName"
               @keyup.enter="fetchPokemon"
               type="text"
               class="form-control"
               placeholder="Enter Pokémon name" />
        <button class="btn btn-outline-secondary" @click="fetchPokemon">Search</button>
      </div>

      <div v-if="pokemonData" class="card mx-auto" style="max-width: 400px;">
        <img :src="pokemonData.sprites.front_default"
             class="card-img-top"
             :alt="pokemonData.name" />
        <div class="card-body">
          <h5 class="card-title text-capitalize">{{ pokemonData.name }}</h5>
          <p class="card-text">
            <strong>Height:</strong> {{ pokemonData.height }}<br />
            <strong>Weight:</strong> {{ pokemonData.weight }}
          </p>
        </div>
      </div>

      <div v-else-if="pokemonNotFound" class="alert alert-warning text-center">
        Pokémon not found.
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue'

  const username = ref('')
  const password = ref('')
  const token = ref('')
  const error = ref('')
  const pokemonName = ref('')
  const pokemonData = ref<any>(null)
  const pokemonNotFound = ref(false)

  async function login() {
    error.value = ''
    if (!username.value || !password.value) {
      error.value = 'Please enter username and password'
      return
    }

    try {
      const res = await fetch('https://localhost:7004/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username: username.value,
          password: password.value
        })
      })

      if (!res.ok) throw new Error('Login failed')
      const data = await res.json()
      token.value = data.token
    } catch (err: any) {
      error.value = err.message || 'Login error'
    }
  }

  async function fetchPokemon() {
    error.value = ''
    pokemonData.value = null
    pokemonNotFound.value = false

    try {
      const res = await fetch(`https://localhost:7004/?name=${pokemonName.value}`, {
        headers: {
          Authorization: `Bearer ${token.value}`
        }
      })

      if (res.status === 404) {
        pokemonNotFound.value = true
        return
      }

      if (!res.ok) throw new Error('Failed to fetch Pokémon')

      pokemonData.value = await res.json()
    } catch (err: any) {
      error.value = err.message || 'Error fetching Pokémon'
    }
  }
</script>

<style scoped>
  body {
    background-color: #f8f9fa;
  }
</style>
